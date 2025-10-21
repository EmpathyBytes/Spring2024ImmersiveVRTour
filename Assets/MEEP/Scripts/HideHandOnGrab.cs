using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class HideHandOnGrab : MonoBehaviour
{
    public SkinnedMeshRenderer handRenderer;
    public HandGrabInteractable interactable;

    private bool wasGrabbed = false;

    private void Update()
    {
        if (interactable != null)
        {
            bool isGrabbed = interactable.Interactors.Count > 0;

            if (isGrabbed && !wasGrabbed)
            {
                // Just started grabbing
                SetHandVisible(false);
                wasGrabbed = true;
            }
            else if (!isGrabbed && wasGrabbed)
            {
                // Just released
                SetHandVisible(true);
                wasGrabbed = false;
            }
        }
    }

    private void SetHandVisible(bool visible)
    {
        if (handRenderer != null)
        {
            handRenderer.enabled = visible;
        }
    }
}
