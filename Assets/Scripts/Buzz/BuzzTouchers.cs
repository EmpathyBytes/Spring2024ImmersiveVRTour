using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Finds the points a visitor can poke buttons with: tracked index fingertips (hand tracking) and
    /// tracked controller tips. Looks inside the scene's "OVRCameraRig" by name, so it needs no SDK types.
    /// </summary>
    public static class BuzzTouchers
    {
        private static readonly List<Transform> tips = new List<Transform>();
        private static float nextScan;
        private static Transform leftTip, rightTip;

        public static List<Transform> Tips()
        {
            if (Time.unscaledTime >= nextScan) { Scan(); nextScan = Time.unscaledTime + 0.75f; }
            return tips;
        }

        private static void Scan()
        {
            tips.Clear();
            var rig = GameObject.Find("OVRCameraRig");
            if (rig == null) return;

            foreach (var t in rig.GetComponentsInChildren<Transform>(false))
            {
                string n = t.name;
                if (n.Contains("IndexTip") || n.Contains("Index_Tip")) tips.Add(t);
            }

            // Controllers count only while they are actually tracked (an idle controller sits at the origin).
            AddControllerTip(rig.transform, "LeftControllerAnchor", XRNode.LeftHand, ref leftTip);
            AddControllerTip(rig.transform, "RightControllerAnchor", XRNode.RightHand, ref rightTip);
        }

        private static void AddControllerTip(Transform rig, string anchorName, XRNode node, ref Transform tip)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid || !device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) || !tracked) return;
            if (tip == null)
            {
                Transform anchor = null;
                foreach (var t in rig.GetComponentsInChildren<Transform>(true)) if (t.name == anchorName) { anchor = t; break; }
                if (anchor == null) return;
                var g = new GameObject("BuzzControllerTip");
                g.transform.SetParent(anchor, false);
                g.transform.localPosition = new Vector3(0f, 0f, 0.07f);   // a little in front of the grip
                tip = g.transform;
            }
            tips.Add(tip);
        }

        /// <summary>True while the given controller button (e.g. the left Y button) is held.</summary>
        public static bool ButtonHeld(XRNode node, InputFeatureUsage<bool> usage)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            return device.isValid && device.TryGetFeatureValue(usage, out bool v) && v;
        }
    }
}
