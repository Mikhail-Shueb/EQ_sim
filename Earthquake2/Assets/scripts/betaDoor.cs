using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRDoorSimple : MonoBehaviour
{
    public enum PivotSide
    {
        Left,
        Right,
        Front,
        Back,
        Center
    }

    public enum RotationAxis
    {
        Y,
        Z,
        X
    }

    [Header("Door Settings")]
    public Transform doorHinge;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    
    [Header("Axis Settings")]
    public RotationAxis doorRotationAxis = RotationAxis.Y; // Renamed to force reset to Y
    public PivotSide pivotSide = PivotSide.Left;
    public Vector3 manualPivotOffset = Vector3.zero;

    [Header("Lock Connection")]
    public bool requireKey = true;
    public VRLock connectedLock;
    
    private bool isOpen = false;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable doorInteractable;
    
    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        
        // 1. ENSURE COLLIDER (Physics Fix)
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogError($"[DoorDebug] Door '{name}' MISSING BoxCollider! Adding one automatically.");
            box = gameObject.AddComponent<BoxCollider>();
        }
        else
        {
            if (box.isTrigger)
            {
                Debug.LogWarning($"[DoorDebug] Door '{name}' collider is a Trigger! Setting to Solid.");
                box.isTrigger = false;
            }
        }

        // 2. AUTO-PIVOT LOGIC (Hinge Fix)
        if (doorHinge == transform)
        {
             if (box != null)
             {
                 Vector3 localHingePos = box.center;
                 Vector3 halfSize = box.size / 2f;

                 switch (pivotSide)
                 {
                     case PivotSide.Left:   localHingePos.x -= halfSize.x; break;
                     case PivotSide.Right:  localHingePos.x += halfSize.x; break;
                     case PivotSide.Front:  localHingePos.z += halfSize.z; break;
                     case PivotSide.Back:   localHingePos.z -= halfSize.z; break;
                     case PivotSide.Center: break; 
                 }
                 
                 localHingePos += manualPivotOffset;

                 Vector3 worldHingePos = transform.TransformPoint(localHingePos);

                 GameObject autoHinge = new GameObject($"{name}_AutoHinge");
                 autoHinge.transform.position = worldHingePos;
                 // FIX: Align Hinge to World up so Y-axis is always vertical!
                 autoHinge.transform.rotation = Quaternion.identity; 

                 transform.SetParent(autoHinge.transform, true);
                 doorHinge = autoHinge.transform;
                 
                 Debug.Log($"[DoorDebug] Created Auto-Hinge ({pivotSide}) at World: {worldHingePos}");
             }
        }

        // 3. INTERACTABLE SETUP
        doorInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (doorInteractable == null)
        {
            doorInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
        
        doorInteractable.selectEntered.AddListener(OnDoorInteracted);
    }
    
    void Update()
    {
        // 4. INTERACTION STATE (Lock Logic)
        if (requireKey && connectedLock != null)
        {
            if (doorInteractable != null)
            {
                bool shouldBeInteractable = connectedLock.IsUnlocked();
                if (doorInteractable.enabled != shouldBeInteractable)
                {
                    doorInteractable.enabled = shouldBeInteractable;
                }
            }
        }
        else
        {
            if (doorInteractable != null && !doorInteractable.enabled)
            {
                doorInteractable.enabled = true;
            }
        }

        // 5. ROTATION ANIMATION (Axis Logic)
        if (Mathf.Abs(currentAngle - targetAngle) > 0.01f)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);
            
            Quaternion rot = Quaternion.identity;
            switch (doorRotationAxis)
            {
                case RotationAxis.X: rot = Quaternion.Euler(currentAngle, 0, 0); break;
                case RotationAxis.Y: rot = Quaternion.Euler(0, currentAngle, 0); break;
                case RotationAxis.Z: rot = Quaternion.Euler(0, 0, currentAngle); break;
            }

            doorHinge.localRotation = rot;
        }
    }
    
    void OnDoorInteracted(SelectEnterEventArgs args)
    {
        TryOpenDoor();
    }
    
    public void TryOpenDoor()
    {
        if (requireKey && connectedLock != null)
        {
            if (!connectedLock.IsUnlocked())
            {
                Debug.Log("Door is locked! You need to insert the correct key.");
                return;
            }
        }
        else if (requireKey && connectedLock == null)
        {
             Debug.LogWarning("Door requires a key but no lock is connected!");
             return;
        }
        
        ToggleDoor();
    }
    
    void ToggleDoor()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? openAngle : 0f;
    }

    void OnDrawGizmos()
    {
        if (doorHinge != null && doorHinge != transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(doorHinge.position, 0.05f);
            Gizmos.DrawLine(doorHinge.position, doorHinge.position + Vector3.up * 0.5f);
        }
    }
}