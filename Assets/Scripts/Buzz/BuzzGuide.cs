using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Prototype AI tour guide ("Buzz").
    /// Answers visitor questions. Tour facts come from the knowledge base (info cards); general Georgia Tech
    /// questions are answered too and tied back to the cards.
    /// - With an API key: asks Claude, with the knowledge base as the trusted source for tour facts.
    /// - Without a key: offline demo mode picks the best-matching knowledge paragraph.
    /// Never commit an API key. A shipped headset build should call a small server that holds the key.
    /// Hook text-to-speech or a UI panel to OnReply.
    /// </summary>
    public class BuzzGuide : MonoBehaviour
    {
        [Header("Knowledge (the only facts Buzz may use)")]
        [SerializeField] private TextAsset knowledgeBase;

        [Header("Claude API (leave key empty for offline demo mode)")]
        [SerializeField] private string model = "claude-sonnet-5-5";
        [SerializeField] private string apiKey = "";
        [SerializeField] private int maxTokens = 300;

        [Header("Speech bubble")]
        [SerializeField] private float bubbleGap = 0.12f;        // space between the top of Buzz and the bubble tip
        [SerializeField] private float bubbleMaxWidth = 1.6f;    // the whole answer is scaled to fit inside this box
        [SerializeField] private float bubbleMaxHeight = 1.1f;
        [SerializeField] private int wrapColumns = 30;

        [Header("Editor demo")]
        [SerializeField] private bool askOnStart = true;
        [SerializeField] private string demoQuestion = "Who was this football presented to?";

        [Serializable] public class ReplyEvent : UnityEvent<string> { }
        public ReplyEvent OnReply = new ReplyEvent();
        /// <summary>Fires when the visitor asks a question (Buzz stops waving and starts answering).</summary>
        public UnityEvent OnAsk = new UnityEvent();

        /// <summary>The most recent answer, so a Repeat button can show or speak it again.</summary>
        public string LastReply { get; private set; } = "";

        private TextMesh bubble;
        private Transform bubbleRoot, panelT, outlineT, tailT, tailOutlineT;
        private MeshRenderer bubbleTextRenderer;
        private Renderer[] bodyRenderers;
        private float headTop = float.NaN;
        private bool layoutDirty;
        private int layoutFrame;
        private const float BaseCharSize = 0.02f;

        private const string SystemPrompt =
            "You are Buzz, the friendly Georgia Tech mascot giving a virtual reality tour. " +
            "Answer in plain language a visitor can listen to, in at most three short sentences and about 40 words total. Put the answer first and leave out extra detail. " +
            "The KNOWLEDGE section holds the tour's info cards. Treat it as fully trusted: when a question is about an object, artist, place or event in it, answer from it. " +
            "For any other question, especially about Georgia Tech, its history, traditions, campus, people or anything else factual, use the web search tool to look it up, then give the actual answer directly and confidently. " +
            "Do not hedge, do not say you are unsure, and do not tell the visitor to ask someone else when search can answer it; only say you could not find it if the search truly returns nothing. " +
            "Do not mention searching, sources or links in your answer, just answer like a knowledgeable guide. " +
            "Only connect an answer back to the tour if it is genuinely relevant. " +
            "Be welcoming to everyone and never judge a question.\n\nKNOWLEDGE:\n";

        [Serializable] private class Msg { public string role; public string content; }
        [Serializable] private class Tool { public string type; public string name; public int max_uses; }
        [Serializable] private class Req { public string model; public int max_tokens; public string system; public Msg[] messages; public Tool[] tools; }
        [Serializable] private class Block { public string type; public string text; }
        [Serializable] private class Resp { public Block[] content; }

        private void Awake()
        {
            // Speech bubble: white rounded-look panel with a dark outline and a tail, floating above Buzz's head.
            var root = new GameObject("BuzzSpeech");
            root.transform.SetParent(transform, false);
            bubbleRoot = root.transform;

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            Material Mat(Color c)
            {
                var m = new Material(shader);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                return m;
            }
            Transform Shape(string name, Mesh mesh, Color c, float z)
            {
                var g = new GameObject(name);
                g.transform.SetParent(bubbleRoot, false);
                g.transform.localPosition = new Vector3(0f, 0f, z);
                g.AddComponent<MeshFilter>().sharedMesh = mesh;
                g.AddComponent<MeshRenderer>().sharedMaterial = Mat(c);
                return g.transform;
            }
            var quad = new Mesh();
            quad.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0), new Vector3(.5f, -.5f, 0) };
            quad.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };   // double-sided
            quad.RecalculateBounds();
            var tri = new Mesh();
            tri.vertices = new[] { new Vector3(-.5f, 0, 0), new Vector3(.5f, 0, 0), new Vector3(0, -1f, 0) };
            tri.triangles = new[] { 0, 1, 2, 0, 2, 1 };
            tri.RecalculateBounds();
            var dark = new Color(0.07f, 0.07f, 0.09f);
            outlineT = Shape("Outline", quad, dark, 0.03f);
            panelT = Shape("Panel", quad, Color.white, 0.02f);
            tailOutlineT = Shape("TailOutline", tri, dark, 0.03f);
            tailT = Shape("Tail", tri, Color.white, 0.02f);

            var go = new GameObject("Text");
            go.transform.SetParent(bubbleRoot, false);
            bubble = go.AddComponent<TextMesh>();
            bubble.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bubbleTextRenderer = go.GetComponent<MeshRenderer>();
            bubbleTextRenderer.sharedMaterial = bubble.font.material;
            bubble.anchor = TextAnchor.MiddleCenter;
            bubble.alignment = TextAlignment.Center;
            bubble.fontSize = 64;
            bubble.characterSize = BaseCharSize;
            bubble.color = new Color(0.07f, 0.07f, 0.09f);
            bubble.text = "";
            bubbleRoot.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (askOnStart && !string.IsNullOrWhiteSpace(demoQuestion)) Ask(demoQuestion);
        }

        private void LateUpdate()
        {
            if (bubbleRoot == null) return;
            var cam = Camera.main;

            // Find Buzz's body so the bubble sits right above his head.
            if (bodyRenderers == null || Time.frameCount % 90 == 0)
            {
                var anim = FindObjectOfType<BuzzCharacterAnimator>();
                if (anim != null) bodyRenderers = anim.GetComponentsInChildren<Renderer>();
            }
            Vector3 anchor = transform.position + Vector3.up * 2.2f;
            if (bodyRenderers != null && bodyRenderers.Length > 0)
            {
                bool first = true; Bounds b = new Bounds();
                foreach (var r in bodyRenderers)
                {
                    if (r == null || !r.enabled) continue;
                    if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
                }
                if (!first)
                {
                    headTop = float.IsNaN(headTop) ? b.max.y : Mathf.Lerp(headTop, b.max.y, Time.deltaTime * 8f);
                    anchor = new Vector3(b.center.x, headTop, b.center.z);
                }
            }
            bubbleRoot.position = anchor + Vector3.up * bubbleGap;
            if (cam != null)
            {
                Vector3 dir = bubbleRoot.position - cam.transform.position; dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f) bubbleRoot.rotation = Quaternion.LookRotation(dir);
            }

            // After the text mesh has been built (one frame after it changes), size the bubble around the whole answer.
            if (layoutDirty && Time.frameCount > layoutFrame)
            {
                var lb = bubbleTextRenderer.localBounds;
                if (lb.size.x > 0.001f && lb.size.y > 0.001f)
                {
                    const float pad = 0.1f, tailH = 0.16f, outline = 0.025f;
                    float s = Mathf.Min(1f, (bubbleMaxWidth - 2f * pad) / lb.size.x, (bubbleMaxHeight - 2f * pad) / lb.size.y);
                    float w = lb.size.x * s + 2f * pad, h = lb.size.y * s + 2f * pad;
                    float cy = tailH + h * 0.5f;
                    panelT.localScale = new Vector3(w, h, 1f); panelT.localPosition = new Vector3(0f, cy, 0.02f);
                    outlineT.localScale = new Vector3(w + 2f * outline, h + 2f * outline, 1f); outlineT.localPosition = new Vector3(0f, cy, 0.03f);
                    tailT.localScale = new Vector3(0.2f, tailH + 0.02f, 1f); tailT.localPosition = new Vector3(0f, tailH + 0.02f, 0.02f);
                    tailOutlineT.localScale = new Vector3(0.2f + 2f * outline, tailH + 0.02f + outline, 1f); tailOutlineT.localPosition = new Vector3(0f, tailH + 0.02f, 0.03f);
                    bubble.transform.localScale = Vector3.one * s;
                    bubble.transform.localPosition = new Vector3(-lb.center.x * s, cy - lb.center.y * s, 0f);
                    bubbleRoot.gameObject.SetActive(true);
                    layoutDirty = false;
                }
            }
        }

        public void Ask(string question)
        {
            StopAllCoroutines();
            OnAsk.Invoke();
            Say("Let me look that up...");
            string key = string.IsNullOrEmpty(apiKey) ? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") : apiKey;
            if (string.IsNullOrEmpty(key)) { Debug.Log("Buzz: no ANTHROPIC_API_KEY found, using offline answers."); Reply(OfflineAnswer(question)); }
            else StartCoroutine(AskClaude(question, key));
        }

        private IEnumerator AskClaude(string question, string key)
        {
            string knowledge = knowledgeBase != null ? knowledgeBase.text : "";
            var req = new Req
            {
                model = model,
                max_tokens = Mathf.Max(maxTokens, 2000), // room for the model's thinking plus a short answer
                system = SystemPrompt + knowledge,
                messages = new[] { new Msg { role = "user", content = question } },
                tools = new[] { new Tool { type = "web_search_20250305", name = "web_search", max_uses = 3 } }
            };
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(req));
            using (var www = new UnityWebRequest("https://api.anthropic.com/v1/messages", "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(body);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("x-api-key", key);
                www.SetRequestHeader("anthropic-version", "2023-06-01");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Buzz: API call failed (" + www.responseCode + " " + www.error + ") " + www.downloadHandler.text + " -> using offline answer.");
                    Reply(OfflineAnswer(question));
                    yield break;
                }
                var resp = JsonUtility.FromJson<Resp>(www.downloadHandler.text);
                string text = null;
                if (resp != null && resp.content != null)
                    foreach (var b in resp.content)
                        if (b != null && !string.IsNullOrEmpty(b.text) && (b.type == null || b.type == "text")) { text = (text == null ? "" : text + "\n") + b.text; }
                if (string.IsNullOrEmpty(text)) Debug.LogWarning("Buzz: could not read answer from response: " + www.downloadHandler.text);
                Reply(string.IsNullOrEmpty(text) ? OfflineAnswer(question) : text);
            }
        }

        // Offline demo: best keyword match among knowledge paragraphs, otherwise admit uncertainty.
        private string OfflineAnswer(string question)
        {
            const string unsure = "I'm not sure about that one. Ask a tour team member!";
            if (knowledgeBase == null) return unsure;
            var words = question.ToLowerInvariant().Split(new[] { ' ', ',', '.', '?', '!', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries);
            string best = null; int bestScore = 0;
            foreach (var para in knowledgeBase.text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string lower = para.ToLowerInvariant();
                int score = 0;
                foreach (var w in words) if (w.Length > 3 && lower.Contains(w)) score++;
                if (score > bestScore) { bestScore = score; best = para.Trim(); }
            }
            return bestScore == 0 ? unsure : best;
        }

        private void Reply(string text)
        {
            LastReply = text;
            Say(text);
            Debug.Log("Buzz says: " + text);
            OnReply.Invoke(text);
        }

        /// <summary>Shows the last answer again (Repeat button).</summary>
        public void RepeatLast() { if (!string.IsNullOrEmpty(LastReply)) Say(LastReply); }

        private void Say(string text)
        {
            if (bubble == null) return;
            // One tidy paragraph: collapse blank lines and runs of spaces, then wrap.
            text = System.Text.RegularExpressions.Regex.Replace(text ?? "", @"\s+", " ").Trim();
            if (text.Length == 0) { bubbleRoot.gameObject.SetActive(false); return; }
            var sb = new StringBuilder(); int col = 0;
            foreach (var word in text.Split(' '))
            {
                if (col + word.Length > wrapColumns && col > 0) { sb.Append('\n'); col = 0; }
                sb.Append(word).Append(' '); col += word.Length + 1;
            }
            bubble.transform.localScale = Vector3.one;
            bubble.text = sb.ToString().TrimEnd();
            layoutDirty = true;
            layoutFrame = Time.frameCount;
        }
    }
}
