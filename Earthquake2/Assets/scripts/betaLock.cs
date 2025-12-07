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
    
    void Update()
    {

        if (!isUnlocked)
        {
            CheckForKeyInsertion();
        }
    }
    
    void CheckForKeyInsertion()
    {

        VRKey[] keys = FindObjectsOfType<VRKey>();
        
        foreach (VRKey key in keys)
        {
            if (key.GetKeyID() == requiredKeyID && key.IsBeingHeld())
            {
                float distance = Vector3.Distance(key.transform.position, keyInsertPoint.position);
                
                if (distance < insertDistance)
                {
                    InsertKey(key);
                    break;
                }
            }
        }
    }
    
    void InsertKey(VRKey key)
    {
        isUnlocked = true;
        insertedKey = key;
        
        key.transform.position = keyInsertPoint.position;
        key.transform.rotation = keyInsertPoint.rotation;
        
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