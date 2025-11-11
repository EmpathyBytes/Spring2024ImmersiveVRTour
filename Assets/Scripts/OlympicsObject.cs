using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class OlympicsObject : MonoBehaviour
{
    private void Awake() =>
        GetComponent<HandGrabInteractable>().WhenStateChanged += OnStateChanged;
    
    private void OnStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select)
        {
            OlympicsTrophyDisplay.Instance.UpdateCollected();
            GetComponent<HandGrabInteractable>().WhenStateChanged -= OnStateChanged;
            Destroy(this);
        }
    }
}
