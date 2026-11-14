using UnityEngine;
using UnityEngine.Events;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Makes the Buzz guide move and react to the visitor. Works on any model (no rig needed):
    /// - idles with a gentle bob,
    /// - turns to face the visitor when they are within notice range,
    /// - walks toward the visitor until a comfortable talking distance,
    /// - hops once and fires OnGreet when the visitor first comes close.
    /// Put it on the same object as BuzzGuide. If 'body' is empty it looks for an object named "Buzz#1".
    /// A rigged model with an Animator can replace the bob/hop later by setting animator triggers.
    /// </summary>
    public class BuzzMotion : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform visitor;            // empty = Camera.main
        [SerializeField] private float noticeRange = 8f;
        [SerializeField] private float stopDistance = 2.0f;
        [SerializeField] private float walkSpeed = 0.9f;
        [SerializeField] private float turnSpeed = 140f;       // degrees per second
        [SerializeField] private float baseHeight = 1.0f;      // body y when standing (statue model pivot sits low)
        [SerializeField] private float bobHeight = 0.04f;
        [SerializeField] private float bobSpeed = 2.0f;
        [SerializeField] private float hopHeight = 0.25f;

        public UnityEvent OnGreet = new UnityEvent();

        private bool greeted;
        private float hopTime = -1f;
        private Vector3 flatPos;

        private void Start()
        {
            if (body == null)
            {
                var found = GameObject.Find("Buzz#1");
                if (found != null) body = found.transform;
            }
            if (body != null) flatPos = new Vector3(body.position.x, 0f, body.position.z);
        }

        /// <summary>Teleports Buzz's ground position (used by the summon button).</summary>
        public void SnapTo(Vector3 worldPosition)
        {
            flatPos = new Vector3(worldPosition.x, FloorY, worldPosition.z);
            greeted = false;
        }

        public Transform Body => body;

        /// <summary>World height of the floor Buzz stands on (the tour rigs stand on the floor, so this is the rig's height).</summary>
        public float FloorY { get; set; }

        /// <summary>True while Buzz is walking toward the visitor (used to drive leg animation).</summary>
        public bool IsWalking { get; private set; }

        private void Update()
        {
            IsWalking = false;
            if (body == null) return;
            if (visitor == null && Camera.main != null) visitor = Camera.main.transform;

            float height = baseHeight + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

            if (visitor != null)
            {
                Vector3 to = visitor.position - flatPos; to.y = 0f;
                float dist = to.magnitude;

                if (dist < noticeRange && dist > 0.01f)
                {
                    // Turn to face the visitor (yaw only; works with the model's own import rotation).
                    Vector3 fwdNow = body.forward; fwdNow.y = 0f;
                    // Model may face another axis, so we turn the world-up yaw by the signed angle to the target.
                    Vector3 faceDir = Vector3.ProjectOnPlane(-body.up, Vector3.up); // statue front is along -up after -90 import
                    if (faceDir.sqrMagnitude < 0.001f) faceDir = fwdNow;
                    float angle = Vector3.SignedAngle(faceDir, to, Vector3.up);
                    float step = Mathf.Clamp(angle, -turnSpeed * Time.deltaTime, turnSpeed * Time.deltaTime);
                    body.rotation = Quaternion.AngleAxis(step, Vector3.up) * body.rotation;

                    if (dist > stopDistance)
                    {
                        flatPos += to.normalized * walkSpeed * Time.deltaTime;
                        IsWalking = true;
                    }

                    if (!greeted && dist < stopDistance + 0.4f)
                    {
                        greeted = true;
                        hopTime = Time.time;
                        OnGreet.Invoke();
                    }
                }
                else if (dist > noticeRange + 2f) greeted = false;
            }

            float hop = 0f;
            if (hopTime >= 0f)
            {
                float t = (Time.time - hopTime) / 0.5f;
                if (t >= 1f) hopTime = -1f; else hop = Mathf.Sin(t * Mathf.PI) * hopHeight;
            }

            body.position = new Vector3(flatPos.x, FloorY + height + hop, flatPos.z);
            transform.position = new Vector3(flatPos.x, FloorY, flatPos.z);   // speech bubble follows
        }
    }
}
