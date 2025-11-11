using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

public class AtariButton : MonoBehaviour
{
    private AtariStation station;

    private void Awake() => station = GetComponentInParent<AtariStation>();

    private void OnTriggerEnter(Collider other)
    {
        var interactor = other.GetComponentInParent<HandGrabInteractor>();
        if (interactor == null || interactor.Hand.Handedness == Handedness.Left)
            return;

        station.PressButton(true);

        transform.localPosition = new(
            transform.localPosition.x,
            0.0554f,
            transform.localPosition.z
        );
    }

    private void OnTriggerExit(Collider other)
    {
        var interactor = other.GetComponentInParent<HandGrabInteractor>();
        if (interactor == null || interactor.Hand.Handedness == Handedness.Left)
            return;

        station.PressButton(false);

        transform.localPosition = new(
            transform.localPosition.x,
            0.0744f,
            transform.localPosition.z
        );
    }
}
