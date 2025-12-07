using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRDoorSimple : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorHinge;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    
    [Header("Lock Connection")]
    public bool requireKey = true;
    public VRLock connectedLock; // <--- NEW: Reference to your lock script
    
    private bool isOpen = false;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable doorInteractable;
    
    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        
        doorInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (doorInteractable == null)
        {
            doorInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
        
        doorInteractable.selectEntered.AddListener(OnDoorInteracted);
    }
    
    void Update()
    {
        if (Mathf.Abs(currentAngle - targetAngle) > 0.01f)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);
            // Rotates around the Y axis
            doorHinge.localRotation = Quaternion.Euler(0, currentAngle, 0);
        }
    }
    
    void OnDoorInteracted(SelectEnterEventArgs args)
    {
        TryOpenDoor();
    }
    
    public void TryOpenDoor()
    {
        // Check the connected lock instead of searching for keys
        if (requireKey && connectedLock != null)
        {
            if (!connectedLock.IsUnlocked())
            {
                Debug.Log("Door is locked! Unlock it first.");
                return;
            }
        }
        
        ToggleDoor();
    }
    
    void ToggleDoor()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? openAngle : 0f;
    }
}