using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Simple world-space info card for an object and its artist. Builds a dark panel with
    /// the title, credit line and description, and turns it to face the visitor.
    /// </summary>
    public class InfoCard : MonoBehaviour
    {
        [SerializeField] private string title = "Albert Einstein";
        [SerializeField] private string credit = "Robert Berks, bronze, cast 2002, installed 2015";
        [TextArea(3, 8)]
        [SerializeField] private string description =
            "Located at the top of Tech Green. The statue was first loaned to the American Museum of Natural History, " +
            "where it was the centerpiece of an exhibition on Einstein, before coming to Georgia Tech.";
        [SerializeField] private string footnote = "";
        [SerializeField] private int wrapColumns = 58;
        [SerializeField] private Vector2 panelSize = new Vector2(1.5f, 0.85f);

        private void Awake()
        {
            var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = "CardPanel";
            Destroy(panel.GetComponent<Collider>());
            panel.transform.SetParent(transform, false);
            panel.transform.localScale = new Vector3(panelSize.x, panelSize.y, 1f);
            var r = panel.GetComponent<MeshRenderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            r.material.color = new Color(0.08f, 0.08f, 0.1f, 1f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            MakeText("Title", title, font, 0.35f, 0.019f, new Color(0.93f, 0.78f, 0.25f), new Vector3(0, panelSize.y * 0.36f, -0.01f), FontStyle.Bold);
            MakeText("Credit", credit, font, 0.2f, 0.011f, new Color(0.75f, 0.75f, 0.8f), new Vector3(0, panelSize.y * 0.18f, -0.01f), FontStyle.Italic);
            MakeText("Body", Wrap(description + (string.IsNullOrEmpty(footnote) ? "" : "\n\n" + footnote)), font, 0.2f, 0.0105f, Color.white, new Vector3(0, panelSize.y * 0.06f, -0.01f), FontStyle.Normal, TextAnchor.UpperCenter);
        }

        private void MakeText(string n, string text, Font font, float anchorUnused, float size, Color color, Vector3 localPos, FontStyle style, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            var tm = go.AddComponent<TextMesh>();
            tm.font = font; tm.text = text; tm.fontSize = 80; tm.characterSize = size;
            tm.anchor = anchor; tm.alignment = TextAlignment.Center; tm.fontStyle = style; tm.color = color;
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        private string Wrap(string text)
        {
            var sb = new System.Text.StringBuilder(); int col = 0;
            foreach (var word in text.Split(' '))
            {
                if (word.Contains("\n")) { sb.Append(word).Append(' '); col = 0; continue; }
                if (col + word.Length > wrapColumns && col > 0) { sb.Append('\n'); col = 0; }
                sb.Append(word).Append(' '); col += word.Length + 1;
            }
            return sb.ToString().TrimEnd();
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 d = transform.position - cam.transform.position; d.y = 0f;
            if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
        }
    }
}
