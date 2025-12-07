using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRKey : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyID = "betaKey";
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isBeingHeld = false;
    
    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }
    
    void OnGrab(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
    }
    
    void OnRelease(SelectExitEventArgs args)
    {
        isBeingHeld = false;
    }
    
    public bool IsBeingHeld()
    {
        return isBeingHeld;
    }
    
    public string GetKeyID()
    {
        return keyID;
    }
}