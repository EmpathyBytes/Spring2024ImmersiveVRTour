using System.Collections;
using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Summon / dismiss logic for Buzz. Summon() puts him a few metres in front of the visitor, he walks up,
    /// waves and says a greeting. In the Unity editor, press B to toggle (sandbox scene only).
    /// In the tour scenes BuzzTourController drives this from the in-world "Ask Buzz" button.
    /// </summary>
    [RequireComponent(typeof(BuzzMotion))]
    public class BuzzSummon : MonoBehaviour
    {
        [SerializeField] private float spawnDistance = 4.5f;
        [SerializeField] private bool startHidden = true;
        [SerializeField] private string greeting = "Hi, I'm Buzz! Poke a question below, or ask me anything about this tour.";

        private BuzzMotion motion;
        private BuzzGuide guide;
        private bool shown;

        public bool IsShown => shown;

        private IEnumerator Start()
        {
            motion = GetComponent<BuzzMotion>();
            guide = GetComponent<BuzzGuide>();
            motion.OnGreet.AddListener(() => { if (shown && guide != null && !string.IsNullOrEmpty(greeting)) guide.ShowText(greeting); });
            yield return null;   // let BuzzMotion find the body first
            if (startHidden && !shown) SetShown(false);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.B)) Toggle();
#endif
        }

        public void Toggle() { if (shown) Dismiss(); else Summon(); }

        public void Summon()
        {
            var cam = Camera.main;
            if (cam == null) cam = FindObjectOfType<Camera>();
            if (cam == null) return;
            if (motion == null) motion = GetComponent<BuzzMotion>();
            Vector3 fwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

            // The tour rigs stand on the floor, so the rig's height is the floor height.
            var rig = GameObject.Find("OVRCameraRig");
            motion.FloorY = rig != null ? rig.transform.position.y : 0f;
            motion.SnapTo(cam.transform.position + fwd * spawnDistance);
            SetShown(true);
        }

        public void Dismiss() { SetShown(false); }

        private void SetShown(bool value)
        {
            shown = value;
            if (motion == null) motion = GetComponent<BuzzMotion>();
            if (motion != null)
            {
                motion.enabled = value;                       // no walking or greeting while he is away
                if (motion.Body != null) motion.Body.gameObject.SetActive(value);
            }
            if (!value && guide != null) guide.HideBubble();
        }
    }
}
