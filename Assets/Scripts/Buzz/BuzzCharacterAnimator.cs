using System.Collections.Generic;
using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Animates the static Buzz_Mascot model (no skeleton) by grouping its named parts under
    /// pivot objects at runtime: waving arm, flapping wings, swaying antennae, head tilt, body sway.
    /// Part names end in an index (Head_0, Upper_arm_22, ...), which is how parts are found.
    /// </summary>
    public class BuzzCharacterAnimator : MonoBehaviour
    {
        [SerializeField] private float waveAngle = 125f;
        [SerializeField] private float wingAngle = 16f;
        [SerializeField] private float wingSpeed = 22f;

        private readonly Dictionary<int, Transform> parts = new Dictionary<int, Transform>();
        private Transform head, antA, antB, wingA, wingB, shoulderA, elbowA, shoulderB;
        private float sA, sB, sWA, sWB, sAntA, sAntB;
        private bool ready;

        public void Build()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                string n = r.gameObject.name;
                int u = n.LastIndexOf('_');
                if (u > 0 && int.TryParse(n.Substring(u + 1), out int id)) parts[id] = r.transform;
            }
            if (!parts.ContainsKey(0) || !parts.ContainsKey(22) || !parts.ContainsKey(43)) return; // not this model

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

            var wbA = Bnd(20, 21); sWA = Side(wbA);
            var wpA = new Vector3(Inner(wbA, sWA), wbA.center.y, wbA.center.z);
            var wbB = Bnd(41, 42); sWB = Side(wbB);
            var wpB = new Vector3(Inner(wbB, sWB), wbB.center.y, wbB.center.z);

            var abA = Bnd(3); sAntA = Side(abA);
            var apA = new Vector3(Inner(abA, sAntA), abA.min.y, abA.center.z);
            var abB = Bnd(11); sAntB = Side(abB);
            var apB = new Vector3(Inner(abB, sAntB), abB.min.y, abB.center.z);

            // Build the pivots and move the parts under them.
            head = Pivot("Head pivot", neckPos, transform);
            Move(new[] { 0, 1, 2, 9, 10 }, head);
            antA = Pivot("Antenna pivot A", apA, head);
            Move(new[] { 3, 4, 5, 6, 7, 8 }, antA);
            antB = Pivot("Antenna pivot B", apB, head);
            Move(new[] { 11, 12, 13, 14, 15, 16 }, antB);

            wingA = Pivot("Wing pivot A", wpA, transform); Move(new[] { 20, 21 }, wingA);
            wingB = Pivot("Wing pivot B", wpB, transform); Move(new[] { 41, 42 }, wingB);

            shoulderA = Pivot("Shoulder pivot A", shA, transform);
            Move(new[] { 22 }, shoulderA);
            elbowA = Pivot("Elbow pivot A", elA, shoulderA);
            Move(new[] { 23, 24, 25, 26, 27, 28, 29, 30 }, elbowA);

            shoulderB = Pivot("Shoulder pivot B", shB, transform);
            Move(new[] { 43, 44, 45, 46, 47, 48, 49, 50, 51 }, shoulderB);

            ready = true;
        }

        private void Update()
        {
            if (!ready) return;
            float t = Time.time;
            transform.localRotation = Quaternion.Euler(0f, 0f, 1.5f * Mathf.Sin(t * 2f));
            head.localRotation = Quaternion.Euler(0f, 7f * Mathf.Sin(t * 1.5f), 3f * Mathf.Sin(t * 2f));
            antA.localRotation = Quaternion.Euler(0f, 0f, 5f * Mathf.Sin(t * 3f));
            antB.localRotation = Quaternion.Euler(0f, 0f, -5f * Mathf.Sin(t * 3f));
            float flap = wingAngle * Mathf.Sin(t * wingSpeed);
            wingA.localRotation = Quaternion.Euler(0f, sWA * flap, 0f);
            wingB.localRotation = Quaternion.Euler(0f, sWB * flap, 0f);
            // Waving arm: raised sideways, forearm wags at the elbow.
            shoulderA.localRotation = Quaternion.Euler(0f, 0f, sA * (waveAngle + 6f * Mathf.Sin(t * 7f)));
            elbowA.localRotation = Quaternion.Euler(0f, 0f, sA * (28f + 18f * Mathf.Sin(t * 7f)));
            // Resting arm: slight sway.
            shoulderB.localRotation = Quaternion.Euler(0f, 0f, sB * 4f * Mathf.Sin(t * 2f));
        }
    }
}
