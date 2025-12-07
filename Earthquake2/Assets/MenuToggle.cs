using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    // Drag your Menu GameObject (the Canvas) here in the Inspector
    [SerializeField] private GameObject menuPanel;

    // Drag the XRI Default Input Actions -> XRI LeftHand Interaction -> Menu Action here
    [SerializeField] private InputActionProperty menuToggleAction;

    private void OnEnable()
    {
        menuToggleAction.action.performed += ToggleMenu;
        menuPanel.SetActive(false); // Start hidden
    }

    private void OnDisable()
    {
        menuToggleAction.action.performed -= ToggleMenu;
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        // Toggles the active state of the menu panel
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}