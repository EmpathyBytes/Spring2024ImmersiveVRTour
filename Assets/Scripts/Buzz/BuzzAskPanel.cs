using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Accessible ways to ask Buzz without speaking: tappable preset questions and a typed question box.
    /// Prototype panel drawn with Unity's immediate-mode GUI, so it works in the editor Game view.
    /// A headset version would use world-space buttons and a virtual keyboard calling the same BuzzGuide.Ask.
    /// Large text and high contrast are options for readability.
    /// </summary>
    [RequireComponent(typeof(BuzzGuide))]
    public class BuzzAskPanel : MonoBehaviour
    {
        [SerializeField] private string[] presetQuestions =
        {
            "Who made this?",
            "When was it installed?",
            "Where on campus is it?",
            "Tell me something about Georgia Tech."
        };
        [SerializeField] private bool largeText = true;
        [SerializeField] private bool highContrast = true;
        [SerializeField] private bool showPanel = true;

        private BuzzGuide guide;
        private string typed = "";
        private Texture2D bg, btn;

        private void Awake()
        {
            guide = GetComponent<BuzzGuide>();
            bg = Solid(new Color(0f, 0f, 0f, 0.85f));
            btn = Solid(new Color(0.93f, 0.78f, 0.25f, 1f));
        }

        private static Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t;
        }

        public void SetPanelVisible(bool visible) { showPanel = visible; }

        private void OnGUI()
        {
            if (!showPanel) return;
            int font = largeText ? 22 : 14;
            float w = Mathf.Min(Screen.width - 40, 560), pad = 12, lineH = font * 2f;
            float h = pad * 4 + lineH * (presetQuestions.Length + 3);
            var box = new Rect(20, Screen.height - h - 20, w, h);

            var panel = new GUIStyle(GUI.skin.box) { normal = { background = highContrast ? bg : null } };
            GUI.Box(box, GUIContent.none, panel);

            var button = new GUIStyle(GUI.skin.button) { fontSize = font, alignment = TextAnchor.MiddleLeft };
            if (highContrast)
            {
                button.normal.background = btn; button.hover.background = btn; button.active.background = btn;
                button.normal.textColor = Color.black; button.hover.textColor = Color.black; button.active.textColor = Color.black;
            }
            float y = box.y + pad;
            foreach (var q in presetQuestions)
            {
                if (GUI.Button(new Rect(box.x + pad, y, w - pad * 2, lineH), q, button)) guide.Ask(q);
                y += lineH + 4;
            }

            float third = (w - pad * 2 - 8) / 3f;
            if (GUI.Button(new Rect(box.x + pad, y, third, lineH), "Repeat", button)) guide.RepeatLast();
            if (GUI.Button(new Rect(box.x + pad + third + 4, y, third, lineH), largeText ? "Text: Large" : "Text: Normal", button)) largeText = !largeText;
            if (GUI.Button(new Rect(box.x + pad + (third + 4) * 2, y, third, lineH), highContrast ? "Contrast: On" : "Contrast: Off", button)) highContrast = !highContrast;
            y += lineH + 4;

            var field = new GUIStyle(GUI.skin.textField) { fontSize = font };
            typed = GUI.TextField(new Rect(box.x + pad, y, w - pad * 2 - 100, lineH), typed, 200, field);
            if (GUI.Button(new Rect(box.x + w - pad - 92, y, 92, lineH), "Ask", button) && !string.IsNullOrWhiteSpace(typed))
            {
                guide.Ask(typed); typed = "";
            }
        }
    }
}
