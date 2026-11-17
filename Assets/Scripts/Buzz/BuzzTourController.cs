using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// The in-tour Buzz experience: a floating "Ask Buzz" button near the visitor's chest (poke it, press the
    /// left controller's Y button, or press B in the Editor). Buzz is built on first use, walks up, waves and
    /// greets; then a floating question panel appears. Poke a question to ask; "Say goodbye" sends him away.
    /// </summary>
    public class BuzzTourController : MonoBehaviour
    {
        private static readonly string[] Questions =
        {
            "What is this room about?",
            "Tell me something about Georgia Tech.",
            "Tell me a fun fact!",
            "What can I do on this tour?"
        };

        private GameObject guideGO;
        private BuzzGuide guide;
        private BuzzSummon summon;
        private BuzzPokeButton summonButton;
        private Transform panel;
        private bool building, wasY, active;
        private Vector3 summonPos, panelPos;
        private bool placed;

        private void Start()
        {
#if UNITY_EDITOR
            Application.runInBackground = true;   // keep the game ticking when the Editor window is not focused
#endif
            // "Ask Buzz" button
            summonButton = BuzzPokeButton.Create(null, "Ask Buzz", new Vector2(0.22f, 0.09f), Vector3.zero,
                new Color(0.93f, 0.78f, 0.25f), new Color(0.07f, 0.07f, 0.09f));
            summonButton.OnPressed.AddListener(Summon);

            // Question panel (hidden until Buzz is here)
            panel = new GameObject("BuzzQuestionPanel").transform;
            float step = 0.1f, y = step * 2.5f;
            foreach (var q in Questions)
            {
                string question = q;
                var b = BuzzPokeButton.Create(panel, question, new Vector2(0.52f, 0.085f), new Vector3(0f, y, 0f),
                    new Color(0.93f, 0.78f, 0.25f), new Color(0.07f, 0.07f, 0.09f));
                b.OnPressed.AddListener(() => { if (guide != null) guide.Ask(question); });
                y -= step;
            }
            var repeat = BuzzPokeButton.Create(panel, "Repeat", new Vector2(0.52f, 0.085f), new Vector3(0f, y, 0f),
                new Color(0.85f, 0.85f, 0.85f), new Color(0.07f, 0.07f, 0.09f));
            repeat.OnPressed.AddListener(() => { if (guide != null) guide.RepeatLast(); });
            y -= step;
            var bye = BuzzPokeButton.Create(panel, "Say goodbye", new Vector2(0.52f, 0.085f), new Vector3(0f, y, 0f),
                new Color(0.55f, 0.55f, 0.58f), Color.white);
            bye.OnPressed.AddListener(Dismiss);
            panel.gameObject.SetActive(false);

        }

        private void Update()
        {
#if UNITY_EDITOR
            EditorFly();
#endif
            var cam = Camera.main;
            if (cam == null) return;

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Space)) { Debug.Log("Buzz: key pressed, toggling summon"); if (active) Dismiss(); else Summon(); }
#endif
            // Left controller Y button toggles Buzz.
            bool y = BuzzTouchers.ButtonHeld(XRNode.LeftHand, CommonUsages.secondaryButton);
            if (y && !wasY) { if (active) Dismiss(); else Summon(); }
            wasY = y;

            // Both UI pieces float at chest height in front of the visitor and glide to follow.
            Vector3 f = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            if (f.sqrMagnitude < 0.001f) f = Vector3.forward;
            Vector3 r = Vector3.Cross(Vector3.up, f);
            Vector3 head = cam.transform.position;
            Vector3 summonTarget = head + f * 0.5f - r * 0.30f + Vector3.down * 0.38f;
            Vector3 panelTarget = head + f * 0.7f + r * 0.05f + Vector3.down * 0.30f;
            if (!placed) { summonPos = summonTarget; panelPos = panelTarget; placed = true; }
            float k = 1f - Mathf.Exp(-3f * Time.deltaTime);
            summonPos = Vector3.Lerp(summonPos, summonTarget, k);
            panelPos = Vector3.Lerp(panelPos, panelTarget, k);
            Face(summonButton.transform, summonPos, head);
            Face(panel, panelPos, head);
        }

        private static void Face(Transform t, Vector3 pos, Vector3 head)
        {
            t.position = pos;
            Vector3 d = pos - head; d.y = 0f;
            if (d.sqrMagnitude > 0.0001f) t.rotation = Quaternion.LookRotation(d);   // +Z away from the visitor, face toward them
        }

        public void Summon()
        {
            Debug.Log("Buzz: summon requested (active=" + active + ", building=" + building + ")");
            if (active || building) return;
            StartCoroutine(SummonRoutine());
        }

        private IEnumerator SummonRoutine()
        {
            building = true;
            if (guideGO == null) BuildGuide();
            if (summon == null) { Debug.LogWarning("Buzz: could not build the guide"); building = false; yield break; }
            yield return null;   // let the new components start
            yield return null;
            summon.Summon();
            Debug.Log("Buzz: summoned");
            active = true;
            building = false;
            summonButton.gameObject.SetActive(false);
            panel.gameObject.SetActive(true);
        }

        public void Dismiss()
        {
            if (summon != null) summon.Dismiss();
            active = false;
            summonButton.gameObject.SetActive(true);
            panel.gameObject.SetActive(false);
        }

        private void BuildGuide()
        {
            guideGO = new GameObject("BuzzTourGuide");
            guideGO.SetActive(false);                                   // set up fields before any Awake runs
            guide = guideGO.AddComponent<BuzzGuide>();
            var motion = guideGO.AddComponent<BuzzMotion>();
            var bee = guideGO.AddComponent<BuzzBee>();
            summon = guideGO.AddComponent<BuzzSummon>();

            Set(guide, "knowledgeBase", Resources.Load<TextAsset>("Buzz/BuzzKnowledge"));
            Set(guide, "askOnStart", false);
            Set(bee, "mascotObj", Resources.Load<TextAsset>("Buzz/Buzz_Mascot_obj"));
            Set(bee, "characterScale", 0.6f);
            Set(bee, "characterFootOffset", 1f);
            Set(motion, "noticeRange", 12f);
            Set(motion, "stopDistance", 2.2f);

            guide.SetContext("The visitor is in the \"" + SceneManager.GetActiveScene().name + "\" area of the Georgia Tech Immersive VR Tour.");
            guideGO.SetActive(true);                                    // Awake: BuzzBee builds the "Buzz#1" body

            // Hide the freshly built body right away (it spawns at the origin) and hand it to BuzzMotion directly,
            // because GameObject.Find cannot find inactive objects.
            var body = GameObject.Find("Buzz#1");
            if (body != null) { Set(motion, "body", body.transform); body.SetActive(false); }
        }

        private static void Set(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null) f.SetValue(target, value);
            else Debug.LogWarning("Buzz: could not set field " + field + " on " + target.GetType().Name);
        }
    }
}
