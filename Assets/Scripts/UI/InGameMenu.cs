using UnityEngine;

/// <summary>
/// TODO: Implement common functionality for in-game menus.
/// Base class for in-game menus that handle opening and closing functionality.
/// </summary>
public abstract class InGameMenu : MonoBehaviour
{
    public virtual void OpenMenu()
    {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible
    }
    public virtual void CloseMenu()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Make the cursor invisible
    }
}