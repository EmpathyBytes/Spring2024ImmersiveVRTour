using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

public class AtariController : MonoBehaviour
{
    [SerializeField]
    private HandGrabInteractable joystick;

    private Vector3 initialPos;
    private bool grabbed;
    private AtariStation station;

    [SerializeField]
    private float radius;

    [SerializeField]
    private Transform joystickPivot;

    private void Awake()
    {
        station = GetComponentInParent<AtariStation>();
        initialPos = joystick.transform.localPosition;

        joystick.WhenStateChanged += args =>
        {
            if (args.NewState == InteractableState.Select)
                grabbed = true;
            else if (args.NewState == InteractableState.Normal)
            {
                grabbed = false;
                joystick.transform.localPosition = initialPos;
                station.MoveJoystick(Vector3.zero);
                joystickPivot.localEulerAngles = Vector3.zero;
            }
        };

        joystick.InjectOptionalInteractorFilters(
            new List<IGameObjectFilter> { new LeftHandFilter() }
        );
    }

    private void LateUpdate()
    {
        if (grabbed)
        {
            var pos = joystick.transform.localPosition;

            pos.y = initialPos.y;
            pos.x = -pos.x;

            var xz = new Vector2(pos.x, pos.z);

            if (xz.magnitude > radius)
                xz = xz.normalized * radius;

            pos.x = xz.x;
            pos.z = xz.y;

            joystick.transform.localPosition = pos;

            var direction = new Vector3(
                (pos.x - initialPos.x) / radius,
                0,
                (pos.z - initialPos.z) / radius
            );

            station.MoveJoystick(direction);
            joystickPivot.localEulerAngles = new(direction.z * 25, 0, direction.x * 25);
        }
    }

    // filters are casted to UnityEngine.Object for some reason
    // found from looking at Interactable.cs cuz there's no documentation
    class LeftHandFilter : Object, IGameObjectFilter
    {
        public bool Filter(GameObject gameObject) =>
            !gameObject.TryGetComponent<HandGrabInteractor>(out var interactor)
            || interactor.Hand?.Handedness == Handedness.Left;
    }
}
