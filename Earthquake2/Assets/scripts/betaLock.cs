using UnityEngine;


public class VRLock : MonoBehaviour
{
    [Header("Lock Settings")]
    public string requiredKeyID = "MainKey";
    public Transform keyInsertPoint;
    public float insertDistance = 0.1f; 
    
    [Header("Visual Feedback")]
    public GameObject lockIndicator; 
    public Material lockedMaterial;
    public Material unlockedMaterial;
    
    private bool isUnlocked = false;
    private VRKey insertedKey = null;
    private Renderer indicatorRenderer;
    
    void Start()
    {
        if (lockIndicator != null)
        {
            indicatorRenderer = lockIndicator.GetComponent<Renderer>();
            UpdateLockVisual();
        }
    }
    
    void LateUpdate()
    {
        // Enforce key position if inserted (Fighting XRI overriding position)
        if (isUnlocked && insertedKey != null && keyInsertPoint != null)
        {
             insertedKey.transform.position = keyInsertPoint.position;
             insertedKey.transform.rotation = keyInsertPoint.rotation;
             
             // Ensure physics stay off
             Rigidbody rb = insertedKey.GetComponent<Rigidbody>();
             if (rb != null && !rb.isKinematic) rb.isKinematic = true;
        }
    }
    
    void Update()
    {
        if (!isUnlocked)
        {
            CheckForKeyInsertion();
        }
    }
    
    void CheckForKeyInsertion()
    {
        if (keyInsertPoint == null) return; // Safety check

        // Use OverlapSphere to detect keys nearby, instead of searching the whole scene
        Collider[] hitColliders = Physics.OverlapSphere(keyInsertPoint.position, 0.2f); // 0.2f radius
        
        // Debug.Log($"[LockDebug] Checking sphere. Found {hitColliders.Length} colliders.");

        foreach (var hitCollider in hitColliders)
        {
            // FIX: VRKey might be on the parent of the collider object
            VRKey key = hitCollider.GetComponentInParent<VRKey>();
            
            if (key != null)
            {
                Debug.Log($"[LockDebug] Found a Key nearby! ID: {key.GetKeyID()} (Required: {requiredKeyID}) | Held: {key.IsBeingHeld()}");

                if (key.GetKeyID() == requiredKeyID && key.IsBeingHeld())
                {
                    // Check distance again if needed, or just trust the sphere
                    float distance = Vector3.Distance(key.transform.position, keyInsertPoint.position);
                    Debug.Log($"[LockDebug] Key Distance: {distance} (Max: {insertDistance})");
                    
                    if (distance < insertDistance)
                    {
                        InsertKey(key);
                        break; 
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (keyInsertPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(keyInsertPoint.position, 0.2f);
        }
    }
    
    void InsertKey(VRKey key)
    {
        isUnlocked = true;
        insertedKey = key;
        
        key.transform.position = keyInsertPoint.position;
        key.transform.rotation = keyInsertPoint.rotation;
        
        // FIX: Parent the key to the lock so it moves with the door
        key.transform.SetParent(keyInsertPoint);

        // FIX: Disable physics so gravity doesn't pull it out
        Rigidbody keyRb = key.GetComponent<Rigidbody>();
        if (keyRb != null)
        {
            keyRb.isKinematic = true;
        }

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = key.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
        
        UpdateLockVisual();
        
        Debug.Log("Key inserted! Lock is now unlocked.");
    }
    
    void UpdateLockVisual()
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material = isUnlocked ? unlockedMaterial : lockedMaterial;
        }
    }
    
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
    
    public void RemoveKey()
    {
        if (insertedKey != null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = insertedKey.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = true;
            }
            insertedKey = null;
        }
        
        isUnlocked = false;
        UpdateLockVisual();
    }
}