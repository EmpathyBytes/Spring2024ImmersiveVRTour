using System.Collections.Generic;
using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Animates the static Buzz_Mascot model (no skeleton) by grouping its named parts under
    /// pivot objects at runtime. Behaviours:
    ///  - idle: head tilt, antenna sway, wing flutter, gentle body sway, arms at rest
    ///  - greet: raises an arm and waves (when BuzzMotion fires OnGreet, on start, and now and then)
    ///  - walk: legs and arms swing while BuzzMotion is walking toward the visitor
    ///  - talk: gestures with the free arm and nods while BuzzGuide is replying
    /// Part names end in an index (Head_0, Upper_arm_22, ...), which is how parts are found.
    /// </summary>
    public class BuzzCharacterAnimator : MonoBehaviour
    {
        [SerializeField] private float waveShoulder = 85f;   // upper arm out to the side, elbow leads
        [SerializeField] private float waveElbow = 55f;     // forearm folds up from the elbow
        [SerializeField] private float wingAngle = 16f;
        [SerializeField] private float wingSpeed = 22f;
        [SerializeField] private float startWaveSeconds = 5f;
        [SerializeField] private float idleWaveEvery = 18f;
        [SerializeField] private float walkRate = 9f;

        private readonly Dictionary<int, Transform> parts = new Dictionary<int, Transform>();
        private Transform head, antA, antB, wingA, wingB, shoulderA, elbowA, shoulderB, elbowB;
        private Transform hipA, hipB, kneeA, kneeB, tail, eyeA, eyeB, chest;
        private Vector3 basePos;
        private float nextBlink, blinkT = -1f, headYaw;
        private float sA, sB, sWA, sWB;
        private bool ready;

        private BuzzMotion motion;
        private float waveUntil, talkUntil, nextIdleWave;
        private float wave, walk, talk, phase;

        public void WaveFor(float seconds) { waveUntil = Time.time + seconds; }
        public void TalkFor(float seconds) { talkUntil = Time.time + seconds; }

        public void Build()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                string n = r.gameObject.name;
                int u = n.LastIndexOf('_');
                if (u > 0 && int.TryParse(n.Substring(u + 1), out int id)) parts[id] = r.transform;
            }
            if (!parts.ContainsKey(0) || !parts.ContainsKey(22) || !parts.ContainsKey(43)) return; // not this model

            basePos = transform.localPosition;
            float cx = transform.position.x;
            Bounds Bnd(params int[] ids)
            {
                bool first = true; Bounds b = new Bounds();
                foreach (int i in ids)
                {
                    if (!parts.TryGetValue(i, out var t)) continue;
                    var r = t.GetComponent<Renderer>(); if (r == null) continue;
                    if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
                }
                return b;
            }
            float Side(Bounds b) { return b.center.x >= cx ? 1f : -1f; }
            float Inner(Bounds b, float s) { return s > 0 ? b.min.x : b.max.x; }
            Transform Pivot(string name, Vector3 world, Transform parent)
            {
                var g = new GameObject(name).transform;
                g.SetParent(parent, false);
                g.position = world;
                return g;
            }
            void Move(IEnumerable<int> ids, Transform to)
            {
                foreach (int i in ids) if (parts.TryGetValue(i, out var t)) t.SetParent(to, true);
            }

            // Work out every pivot position first, while everything is still unparented.
            var hb = Bnd(0, 1, 2, 9, 10);
            var neckPos = new Vector3(cx, hb.min.y + hb.size.y * 0.15f, hb.center.z);

            var uaA = Bnd(22); sA = Side(uaA);
            var shA = new Vector3(Inner(uaA, sA), uaA.max.y - uaA.size.y * 0.1f, uaA.center.z);
            var faA = Bnd(23); var elA = new Vector3(Inner(faA, sA), faA.max.y - faA.size.y * 0.1f, faA.center.z);
            var uaB = Bnd(43); sB = Side(uaB);
            var shB = new Vector3(Inner(uaB, sB), uaB.max.y - uaB.size.y * 0.1f, uaB.center.z);
            var faB = Bnd(44); var elB = new Vector3(Inner(faB, sB), faB.max.y - faB.size.y * 0.1f, faB.center.z);

            var wbA = Bnd(20, 21); sWA = Side(wbA);
            var wpA = new Vector3(Inner(wbA, sWA), wbA.center.y, wbA.center.z);
            var wbB = Bnd(41, 42); sWB = Side(wbB);
            var wpB = new Vector3(Inner(wbB, sWB), wbB.center.y, wbB.center.z);

            var abA = Bnd(3); var apA = new Vector3(Inner(abA, Side(abA)), abA.min.y, abA.center.z);
            var abB = Bnd(11); var apB = new Vector3(Inner(abB, Side(abB)), abB.min.y, abB.center.z);

            var thA = Bnd(31); var hpA = new Vector3(thA.center.x, thA.max.y - thA.size.y * 0.05f, thA.center.z);
            var clA = Bnd(32); var kpA = new Vector3(clA.center.x, clA.max.y - clA.size.y * 0.05f, clA.center.z);
            var thB = Bnd(52); var hpB = new Vector3(thB.center.x, thB.max.y - thB.size.y * 0.05f, thB.center.z);
            var clB = Bnd(53); var kpB = new Vector3(clB.center.x, clB.max.y - clB.size.y * 0.05f, clB.center.z);

            // Build the pivots and move the parts under them.
            head = Pivot("Head pivot", neckPos, transform);
            Move(new[] { 0, 1, 2, 9, 10 }, head);
            antA = Pivot("Antenna pivot A", apA, head); Move(new[] { 3, 4, 5, 6, 7, 8 }, antA);
            antB = Pivot("Antenna pivot B", apB, head); Move(new[] { 11, 12, 13, 14, 15, 16 }, antB);

            wingA = Pivot("Wing pivot A", wpA, transform); Move(new[] { 20, 21 }, wingA);
            wingB = Pivot("Wing pivot B", wpB, transform); Move(new[] { 41, 42 }, wingB);

            shoulderA = Pivot("Shoulder pivot A", shA, transform); Move(new[] { 22 }, shoulderA);
            elbowA = Pivot("Elbow pivot A", elA, shoulderA); Move(new[] { 23, 24, 25, 26, 27, 28, 29, 30 }, elbowA);
            shoulderB = Pivot("Shoulder pivot B", shB, transform); Move(new[] { 43 }, shoulderB);
            elbowB = Pivot("Elbow pivot B", elB, shoulderB); Move(new[] { 44, 45, 46, 47, 48, 49, 50, 51 }, elbowB);

            hipA = Pivot("Hip pivot A", hpA, transform); Move(new[] { 31 }, hipA);
            kneeA = Pivot("Knee pivot A", kpA, hipA); Move(new[] { 32, 33, 34, 35, 36, 37, 38, 39, 40 }, kneeA);
            hipB = Pivot("Hip pivot B", hpB, transform); Move(new[] { 52 }, hipB);
            kneeB = Pivot("Knee pivot B", kpB, hipB); Move(new[] { 53, 54, 55, 56, 57, 58, 59, 60, 61 }, kneeB);

            var abd = Bnd(19); var cst = Bnd(17, 62, 63, 64, 65);
            tail = Pivot("Tail pivot", Vector3.Lerp(abd.center, cst.center, 0.6f), transform); Move(new[] { 19 }, tail);
            chest = Pivot("Chest pivot", cst.center, transform); Move(new[] { 17, 62, 63, 64, 65 }, chest);
            var eAb = Bnd(1, 2); var eBb = Bnd(9, 10);
            eyeA = Pivot("Eye pivot A", eAb.center, head); Move(new[] { 1, 2 }, eyeA);
            eyeB = Pivot("Eye pivot B", eBb.center, head); Move(new[] { 9, 10 }, eyeB);
            nextBlink = Time.time + 2f;

            // Hook into the guide so he waves on greeting and gestures while answering.
            motion = FindObjectOfType<BuzzMotion>();
            if (motion != null) motion.OnGreet.AddListener(() => WaveFor(4f));
            var guide = FindObjectOfType<BuzzGuide>();
            if (guide != null) guide.OnReply.AddListener(text => TalkFor(Mathf.Clamp((text?.Length ?? 0) / 14f, 2f, 12f)));

            WaveFor(startWaveSeconds);
            nextIdleWave = Time.time + idleWaveEvery;
            ready = true;
        }

        private void Update()
        {
            if (!ready) return;
            float t = Time.time, dt = Time.deltaTime;

            if (t > nextIdleWave) { WaveFor(3f); nextIdleWave = t + idleWaveEvery; }
            bool walking = motion != null && motion.IsWalking;
            wave = Mathf.MoveTowards(wave, (t < waveUntil && !walking) ? 1f : 0f, dt * 3f);
            walk = Mathf.MoveTowards(walk, walking ? 1f : 0f, dt * 5f);
            talk = Mathf.MoveTowards(talk, (t < talkUntil && !walking) ? 1f : 0f, dt * 4f);
            if (walk > 0.01f) phase += dt * walkRate;

            float swing = Mathf.Sin(phase) * walk;
            float act = Mathf.Max(walk, Mathf.Max(wave, talk));

            // Body: sway, bounce while walking, breathing.
            float bob = 0.012f * Mathf.Abs(Mathf.Sin(phase)) * walk + 0.004f * Mathf.Sin(t * 2.2f);
            transform.localPosition = basePos + new Vector3(0f, bob, 0f);
            transform.localRotation = Quaternion.Euler(0f, 3f * wave * Mathf.Sin(t * 3f), 1.5f * Mathf.Sin(t * 2f) + 2.5f * wave * Mathf.Sin(t * 3f));
            float breathe = 1f + 0.015f * Mathf.Sin(t * 2.4f);
            chest.localScale = new Vector3(breathe, breathe, breathe);

            // Head: looks toward the visitor, nods when talking, bobs when waving.
            float targetYaw = 0f;
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 to = transform.InverseTransformPoint(cam.transform.position);
                targetYaw = Mathf.Clamp(Mathf.Atan2(to.x, Mathf.Abs(to.z) + 0.001f) * Mathf.Rad2Deg * 0.5f, -35f, 35f);
            }
            headYaw = Mathf.LerpAngle(headYaw, targetYaw, dt * 3f);
            head.localRotation = Quaternion.Euler(talk * 6f * Mathf.Sin(t * 5f) + wave * 4f * Mathf.Sin(t * 4f),
                                                  headYaw + 5f * Mathf.Sin(t * 1.5f),
                                                  3f * Mathf.Sin(t * 2f) + wave * 6f * Mathf.Sin(t * 4f));

            // Blink.
            if (blinkT < 0f && t > nextBlink) { blinkT = 0f; nextBlink = t + Random.Range(2.5f, 5f); }
            float eyeY = 1f;
            if (blinkT >= 0f) { blinkT += dt; eyeY = 1f - Mathf.Sin(Mathf.Clamp01(blinkT / 0.16f) * Mathf.PI) * 0.9f; if (blinkT > 0.16f) blinkT = -1f; }
            eyeA.localScale = new Vector3(1f, eyeY, 1f);
            eyeB.localScale = new Vector3(1f, eyeY, 1f);

            // Antennae bounce more when active; tail wags.
            float ant = 5f + 9f * act;
            antA.localRotation = Quaternion.Euler(4f * act * Mathf.Sin(t * 6f), 0f, ant * Mathf.Sin(t * 3f));
            antB.localRotation = Quaternion.Euler(4f * act * Mathf.Sin(t * 6f + 1f), 0f, -ant * Mathf.Sin(t * 3f + 0.5f));
            tail.localRotation = Quaternion.Euler(4f * Mathf.Sin(t * 2.3f), (6f + 10f * act) * Mathf.Sin(t * (3f + 3f * act)), 0f);

            // Wings: faster and wider when excited.
            float flap = wingAngle * (1f + 0.6f * Mathf.Max(walk, wave)) * Mathf.Sin(t * wingSpeed);
            wingA.localRotation = Quaternion.Euler(0f, sWA * flap, 0f);
            wingB.localRotation = Quaternion.Euler(0f, sWB * flap, 0f);

            // Arm A wave: the ELBOW leads. Upper arm lifts out to the side (never past the head),
            // forearm folds up from the elbow and the hand waves side to side.
            float rest = -8f; // arms hang slightly away from the body
            float lift = Mathf.Lerp(rest, waveShoulder, wave);
            shoulderA.localRotation = Quaternion.Euler(38f * wave - 26f * swing, 0f, sA * (lift + 3f * wave * Mathf.Sin(t * 4f)));
            elbowA.localRotation = Quaternion.Euler(0f, 0f, sA * wave * (waveElbow + 22f * Mathf.Sin(t * 8f)));

            // Arm B: swings when walking, gestures while talking, otherwise rests with a slight sway.
            float still = 1f - wave; // other arm stays put while one waves
            float gesture = still * talk * (32f + 14f * Mathf.Sin(t * 4f));
            shoulderB.localRotation = Quaternion.Euler(26f * swing, 0f, sB * (rest + still * 3f * Mathf.Sin(t * 2f) + gesture));
            elbowB.localRotation = Quaternion.Euler(0f, 0f, sB * still * talk * (35f + 18f * Mathf.Sin(t * 4f + 1f)));

            // Legs: alternate hip swing, knees bend as the leg comes forward.
            hipA.localRotation = Quaternion.Euler(30f * swing, 0f, 0f);
            hipB.localRotation = Quaternion.Euler(-30f * swing, 0f, 0f);
            kneeA.localRotation = Quaternion.Euler(38f * walk * Mathf.Max(0f, -Mathf.Cos(phase)), 0f, 0f);
            kneeB.localRotation = Quaternion.Euler(38f * walk * Mathf.Max(0f, Mathf.Cos(phase)), 0f, 0f);
        }
    }
}
