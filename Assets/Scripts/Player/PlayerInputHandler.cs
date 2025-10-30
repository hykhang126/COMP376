using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player), typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }

    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        PlayerInput = GetComponent<PlayerInput>();

        Init();
    }

    private void Init()
    {
        PlayerInput.actions["Move"].performed += OnMove;
        PlayerInput.actions["Move"].canceled += OnMove;
        PlayerInput.actions["Look"].performed += OnLook;
        PlayerInput.actions["Look"].canceled += OnLook;
        PlayerInput.actions["Sprint"].performed += OnSprint;
        PlayerInput.actions["Sprint"].canceled += OnSprint;
        PlayerInput.actions["Crouch"].performed += OnCrouch;
        PlayerInput.actions["Crouch"].canceled += OnCrouch;
    }

    #region Input Maps Control

    public void EnableInput()
    {
        PlayerInput.enabled = true;
    }

    public void DisableInput()
    {
        PlayerInput.enabled = false;
    }

    public void SwitchInputMap(string mapName)
    {
        PlayerInput.SwitchCurrentActionMap(mapName);
    }

    public string GetCurrentInputMap()
    {
        return PlayerInput.currentActionMap.name;
    }

    public void ToggleGrabInput(bool enable)
    {
        if (enable)
        {
            PlayerInput.actions["Grab"].performed += OnGrab;
        }
        else
        {
            PlayerInput.actions["Grab"].performed -= OnGrab;
        }
    }

    #endregion

    #region Inputs

    public void OnMove(InputAction.CallbackContext context)
    {
        player.movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        player.lookInput = context.ReadValue<Vector2>();
        if (!player.isInverted)
        {
            //Invert to adjust for inverted camera input
            player.lookInput.y *= -1f;
        }
        player.OnLook(); // Call Player's OnLook to process the input
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        bool isSprinting = context.ReadValueAsButton();
        player.OnSprint(isSprinting); // Call Player's OnSprint to process the input
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        bool isCrouching = context.ReadValueAsButton();
        player.OnCrouch(isCrouching); // Call Player's OnCrouch to process the input
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (player.IsHoldingItem())
        {
            player.DropItem();
        }
    }

    #endregion

    #region InMenu Callbacks

    public void InMenuEnter()
    {
        PlayerInput.actions["Move"].performed -= OnMove;
        PlayerInput.actions["Move"].canceled -= OnMove;
        PlayerInput.actions["Look"].performed -= OnLook;
        PlayerInput.actions["Look"].canceled -= OnLook;
        PlayerInput.actions["Crouch"].performed -= OnCrouch;
        PlayerInput.actions["Crouch"].canceled -= OnCrouch;
        PlayerInput.actions["Sprint"].performed -= OnSprint;
        PlayerInput.actions["Sprint"].canceled -= OnSprint;

    }

    public void InMenuExit()
    {
        PlayerInput.actions["Move"].performed += OnMove;
        PlayerInput.actions["Move"].canceled += OnMove;
        PlayerInput.actions["Look"].performed += OnLook;
        PlayerInput.actions["Look"].canceled += OnLook;
        PlayerInput.actions["Crouch"].performed += OnCrouch;
        PlayerInput.actions["Crouch"].canceled += OnCrouch;
        PlayerInput.actions["Sprint"].performed += OnSprint;
        PlayerInput.actions["Sprint"].canceled += OnSprint;

    }

    #endregion

}
