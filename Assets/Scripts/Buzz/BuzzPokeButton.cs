using UnityEngine;
using UnityEngine.Events;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// A world-space button pressed by poking it with a fingertip or controller tip (same idea as the tour's
    /// exit button). In the Editor you can also click it with the mouse. The button faces -Z; build it with Create.
    /// </summary>
    public class BuzzPokeButton : MonoBehaviour
    {
        public UnityEvent OnPressed = new UnityEvent();
        public Vector2 Size { get; private set; }

        private TextMesh label;
        private MeshRenderer labelRenderer;
        private Transform fillT;
        private Material fillMat;
        private Color idleColor, hoverColor;
        private bool armed = true, fitPending;
        private int fitFrame;
        private float lastPress = -10f;

        private static Shader shader;
        private static Mesh quad;
        private const float Cooldown = 0.7f;

        public static BuzzPokeButton Create(Transform parent, string text, Vector2 size, Vector3 localPos, Color fill, Color textColor)
        {
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Unlit/Color");
            }
            if (quad == null)
            {
                quad = new Mesh();
                quad.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0), new Vector3(.5f, -.5f, 0) };
                quad.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };
                quad.RecalculateBounds();
            }

            var go = new GameObject("Button: " + text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var b = go.AddComponent<BuzzPokeButton>();
            b.Size = size;
            b.idleColor = fill;
            b.hoverColor = Color.Lerp(fill, Color.white, 0.45f);

            Material Mat(Color c)
            {
                var m = new Material(shader);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                return m;
            }
            Transform Quad(string n, Vector2 s, float z, Material m)
            {
                var g = new GameObject(n);
                g.transform.SetParent(go.transform, false);
                g.transform.localPosition = new Vector3(0f, 0f, z);
                g.transform.localScale = new Vector3(s.x, s.y, 1f);
                g.AddComponent<MeshFilter>().sharedMesh = quad;
                g.AddComponent<MeshRenderer>().sharedMaterial = m;
                return g.transform;
            }
            Quad("Outline", size + new Vector2(0.012f, 0.012f), 0.004f, Mat(new Color(0.07f, 0.07f, 0.09f)));
            b.fillMat = Mat(fill);
            b.fillT = Quad("Fill", size, 0.002f, b.fillMat);

            var t = new GameObject("Label");
            t.transform.SetParent(go.transform, false);
            b.label = t.AddComponent<TextMesh>();
            b.label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            b.labelRenderer = t.GetComponent<MeshRenderer>();
            b.labelRenderer.sharedMaterial = b.label.font.material;
            b.label.anchor = TextAnchor.MiddleCenter;
            b.label.alignment = TextAlignment.Center;
            b.label.fontSize = 64;
            b.label.characterSize = 0.008f;
            b.label.color = textColor;
            b.SetLabel(text);
            return b;
        }

        public void SetLabel(string text)
        {
            label.text = text;
            label.transform.localScale = Vector3.one;
            fitPending = true;
            fitFrame = Time.frameCount;
        }

        private void Update()
        {
            // Shrink the label if needed so it always fits inside the button (one frame after the text changes).
            if (fitPending && Time.frameCount > fitFrame)
            {
                var lb = labelRenderer.localBounds;
                if (lb.size.x > 0.001f)
                {
                    float s = Mathf.Min(1f, (Size.x - 0.05f) / lb.size.x, (Size.y - 0.02f) / Mathf.Max(lb.size.y, 0.001f));
                    label.transform.localScale = Vector3.one * s;
                    label.transform.localPosition = new Vector3(-lb.center.x * s, -lb.center.y * s, -0.001f);
                    fitPending = false;
                }
            }

            bool hovering = false, inside = false;
            foreach (var tip in BuzzTouchers.Tips())
            {
                if (tip == null) continue;
                Vector3 p = transform.InverseTransformPoint(tip.position);
                bool overFace = Mathf.Abs(p.x) < Size.x * 0.5f + 0.01f && Mathf.Abs(p.y) < Size.y * 0.5f + 0.01f;
                if (!overFace) continue;
                if (p.z > -0.07f && p.z < 0.04f) { hovering = true; }
                if (p.z > -0.015f && p.z < 0.04f) inside = true;        // fingertip has pushed into the button
            }
            if (!hovering) armed = true;                               // re-arm once the finger leaves
            if (inside && armed && Time.time - lastPress > Cooldown) { armed = false; Press(); }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0) && Camera.main != null && Time.time - lastPress > Cooldown)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (new Plane(transform.forward, transform.position).Raycast(ray, out float d))
                {
                    Vector3 p = transform.InverseTransformPoint(ray.GetPoint(d));
                    if (Mathf.Abs(p.x) < Size.x * 0.5f && Mathf.Abs(p.y) < Size.y * 0.5f) Press();
                }
            }
#endif
            Color target = hovering ? hoverColor : idleColor;
            if (fillMat.HasProperty("_BaseColor")) fillMat.SetColor("_BaseColor", target);
            if (fillMat.HasProperty("_Color")) fillMat.SetColor("_Color", target);
            float press = (Time.time - lastPress) < 0.15f ? 0.94f : 1f;
            fillT.localScale = new Vector3(Size.x * press, Size.y * press, 1f);
        }

        private void Press()
        {
            lastPress = Time.time;
            OnPressed.Invoke();
        }
    }
}
