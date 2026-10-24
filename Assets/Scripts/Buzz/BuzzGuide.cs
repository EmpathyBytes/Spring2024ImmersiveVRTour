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
    /// Answers visitor questions using ONLY the facts in the knowledge base (retrieval-grounding).
    /// - With an API key: asks Claude, with the knowledge base as the only allowed source.
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
        [SerializeField] private Vector3 bubbleOffset = new Vector3(0f, 2.2f, 0f);
        [SerializeField] private float characterSize = 0.03f;
        [SerializeField] private int wrapColumns = 34;

        [Header("Editor demo")]
        [SerializeField] private bool askOnStart = true;
        [SerializeField] private string demoQuestion = "Who was this football presented to?";

        [Serializable] public class ReplyEvent : UnityEvent<string> { }
        public ReplyEvent OnReply = new ReplyEvent();

        private TextMesh bubble;

        private const string SystemPrompt =
            "You are Buzz, the friendly Georgia Tech mascot giving a virtual reality tour. " +
            "Answer in one to three short sentences. Use ONLY the facts in the KNOWLEDGE section. " +
            "If the answer is not in the knowledge, say you are not sure and suggest asking a tour team member. " +
            "Never invent dates, names or history.\n\nKNOWLEDGE:\n";

        [Serializable] private class Msg { public string role; public string content; }
        [Serializable] private class Req { public string model; public int max_tokens; public string system; public Msg[] messages; }
        [Serializable] private class Block { public string type; public string text; }
        [Serializable] private class Resp { public Block[] content; }

        private void Awake()
        {
            var go = new GameObject("BuzzSpeech");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = bubbleOffset;
            bubble = go.AddComponent<TextMesh>();
            bubble.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().sharedMaterial = bubble.font.material;
            bubble.anchor = TextAnchor.LowerCenter;
            bubble.alignment = TextAlignment.Center;
            bubble.fontSize = 64;
            bubble.characterSize = characterSize;
            bubble.color = Color.white;
            bubble.text = "";
        }

        private void Start()
        {
            if (askOnStart && !string.IsNullOrWhiteSpace(demoQuestion)) Ask(demoQuestion);
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (bubble != null && cam != null)
                bubble.transform.rotation = Quaternion.LookRotation(bubble.transform.position - cam.transform.position);
        }

        public void Ask(string question)
        {
            StopAllCoroutines();
            Say("Hmm, let me think...");
            string key = string.IsNullOrEmpty(apiKey) ? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") : apiKey;
            if (string.IsNullOrEmpty(key)) Reply(OfflineAnswer(question));
            else StartCoroutine(AskClaude(question, key));
        }

        private IEnumerator AskClaude(string question, string key)
        {
            string knowledge = knowledgeBase != null ? knowledgeBase.text : "";
            var req = new Req
            {
                model = model,
                max_tokens = maxTokens,
                system = SystemPrompt + knowledge,
                messages = new[] { new Msg { role = "user", content = question } }
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
                    Debug.LogWarning("Buzz: API call failed (" + www.error + "), using offline answer.");
                    Reply(OfflineAnswer(question));
                    yield break;
                }
                var resp = JsonUtility.FromJson<Resp>(www.downloadHandler.text);
                string text = resp != null && resp.content != null && resp.content.Length > 0 ? resp.content[0].text : null;
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
            Say(text);
            Debug.Log("Buzz says: " + text);
            OnReply.Invoke(text);
        }

        private void Say(string text)
        {
            if (bubble == null) return;
            var sb = new StringBuilder(); int col = 0;
            foreach (var word in text.Split(' '))
            {
                if (col + word.Length > wrapColumns && col > 0) { sb.Append('\n'); col = 0; }
                sb.Append(word).Append(' '); col += word.Length + 1;
            }
            bubble.text = sb.ToString().TrimEnd();
        }
    }
}
