using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRKey : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyID = "betaKey";
    
    [Header("Hitbox Settings")]
    public float hitboxScale = 1.5f; // Increase size by 50%

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isBeingHeld = false;
    
    void Awake()
    {
        // Increase hitbox size for easier grabbing
        // FIX: Search in children because the collider might be on a child mesh (SM_Key)
        Collider col = GetComponentInChildren<Collider>();
        
        if (col != null)
        {
            // Increase hitbox size for easier grabbing
            // Using standard casting for maximum compatibility
            
            BoxCollider boxCol = col as BoxCollider;
            if (boxCol != null)
            {
                boxCol.size *= hitboxScale;
            }
            else
            {
                SphereCollider sphereCol = col as SphereCollider;
                if (sphereCol != null)
                {
                    sphereCol.radius *= hitboxScale;
                }
                else
                {
                    CapsuleCollider capsuleCol = col as CapsuleCollider;
                    if (capsuleCol != null)
                    {
                        capsuleCol.radius *= hitboxScale;
                        capsuleCol.height *= hitboxScale;
                    }
                }
            }
        }

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