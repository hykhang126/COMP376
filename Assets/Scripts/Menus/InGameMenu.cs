using UnityEngine;

public abstract class InGameMenu : MonoBehaviour
{
    public InputSystem_Actions action;

    private void OnEnable()
    {
        action.Enable();
    }
    private void OnDisable()
    {
        action.Disable();
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible
    }
    public void CloseMenu()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Make the cursor invisible
    }
}