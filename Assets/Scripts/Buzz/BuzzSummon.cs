using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Summon button for Buzz. Call Toggle() / Summon() / Dismiss() from any button
    /// (for example the hand-menu button's UnityEvent). In the Unity editor, press B to toggle.
    /// Buzz appears in front of the visitor, wherever they are in the tour.
    /// </summary>
    [RequireComponent(typeof(BuzzMotion))]
    public class BuzzSummon : MonoBehaviour
    {
        [SerializeField] private float distanceInFront = 1.5f;
        [SerializeField] private bool startHidden = true;

        private BuzzMotion motion;
        private bool shown;

        private void Start()
        {
            motion = GetComponent<BuzzMotion>();
            if (startHidden) SetShown(false);
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
            if (cam == null || motion == null) return;
            Vector3 fwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            motion.SnapTo(cam.transform.position + fwd * distanceInFront);
            SetShown(true);
        }

        public void Dismiss() { SetShown(false); }

        private void SetShown(bool value)
        {
            shown = value;
            if (motion != null && motion.Body != null) motion.Body.gameObject.SetActive(value);
            // The speech bubble is a child of this object; show it only while Buzz is out.
            foreach (Transform child in transform) child.gameObject.SetActive(value);
        }
    }
}
