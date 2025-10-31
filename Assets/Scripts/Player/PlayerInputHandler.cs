using System;
using UnityEngine;
using UnityEngine.InputSystem;

/**
TODO: Make the handler switch maps based on Player State changes!
**/

/// <summary>
/// Handles player input using the Unity Input System.
/// Provides global access to PlayerInput, modifying its behavior and managing input action subscriptions.
/// Accessible via Player.InstanceReference.playerInputHandler
/// </summary>
[RequireComponent(typeof(Player), typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    // Global
    // Action names
    [Header("Input Action Names")]
    [Tooltip("Name of the Move action in the Player Input Action Map")]
    public string moveAction = "Move";
    [Tooltip("Name of the Look action in the Player Input Action Map")]
    public string lookAction = "Look";
    [Tooltip("Name of the Sprint action in the Player Input Action Map")]
    public string sprintAction = "Sprint";
    [Tooltip("Name of the Crouch action in the Player Input Action Map")]
    public string crouchAction = "Crouch";
    [Tooltip("Name of the Grab action in the Player Input Action Map")]
    public string grabAction = "Grab";
    [Tooltip("Name of the Interact action in the Player Input Action Map")]
    public string interactionAction = "Interact";
    // Map names
    [Header("Input Action Map Names")]
    [Tooltip("Name of the Player Action Map")]
    public string playerActionMap = "Player";
    [Tooltip("Name of the Inventory Action Map")]
    public string inventoryActionMap = "Inventory";
    [Tooltip("Name of the Pause Action Map")]
    public string pauseActionMap = "Pause";


    // Private
    // Component reference
    private PlayerInput PlayerInput { get; set; }
    // Script reference
    private PlayerInput_Actions PlayerInputActions { get; set; }
    private Player player;

    // DEBUG
    [Header("DEBUG: No need to assign")]
    public string currentActionMapName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        PlayerInput = GetComponent<PlayerInput>();

        PlayerInputActions = new PlayerInput_Actions();
        PlayerInputActions.Enable();

        Init();
    }

    private void Init()
    {
        // Movement and Look
        PlayerInput.actions[moveAction].performed += OnMove;
        PlayerInput.actions[moveAction].canceled += OnMove;
        PlayerInput.actions[lookAction].performed += OnLook;
        PlayerInput.actions[lookAction].canceled += OnLook;
        PlayerInput.actions[sprintAction].performed += OnSprint;
        PlayerInput.actions[sprintAction].canceled += OnSprint;
        PlayerInput.actions[crouchAction].performed += OnCrouch;
        PlayerInput.actions[crouchAction].canceled += OnCrouch;

        // Grab
        PlayerInput.actions[grabAction].performed += OnGrab;
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
            PlayerInput.actions[grabAction].performed += OnGrab;
        }
        else
        {
            PlayerInput.actions[grabAction].performed -= OnGrab;
        }
    }

    public void AddActionSubscriber(string actionName, Action<InputAction.CallbackContext> callback)
    {
        // Unity will handle null checks internally
        InputAction action = PlayerInput.actions.FindAction(actionName);
        action.performed += callback;
    }

    public void RemoveActionSubscriber(string actionName, Action<InputAction.CallbackContext> callback)
    {
        // Unity will handle null checks internally
        InputAction action = PlayerInput.actions.FindAction(actionName);
        action.performed -= callback;
    }

    public void AddMapActionNoParamSubscriber(string mapName, string actionName, Action callback)
    {
        InputActionMap actionMap = PlayerInput.actions.FindActionMap(mapName);
        InputAction action = actionMap.FindAction(actionName);
        action.performed += _ => callback();
    }

    public void RemoveMapActionNoParamSubscriber(string mapName, string actionName, Action callback)
    {
        InputActionMap actionMap = PlayerInput.actions.FindActionMap(mapName);
        InputAction action = actionMap.FindAction(actionName);
        action.performed -= _ => callback();
    }

    public void AddMapActionSubscriber(string mapName, string actionName, Action<InputAction.CallbackContext> callback)
    {
        InputActionMap actionMap = PlayerInput.actions.FindActionMap(mapName);
        InputAction action = actionMap.FindAction(actionName);
        action.performed += callback;
    }

    public void RemoveMapActionSubscriber(string mapName, string actionName, Action<InputAction.CallbackContext> callback)
    {
        InputActionMap actionMap = PlayerInput.actions.FindActionMap(mapName);
        InputAction action = actionMap.FindAction(actionName);
        action.performed -= callback;
    }

    #endregion

    #region Inputs Callbacks

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
        PlayerInput.actions[moveAction].performed -= OnMove;
        PlayerInput.actions[moveAction].canceled -= OnMove;
        PlayerInput.actions[lookAction].performed -= OnLook;
        PlayerInput.actions[lookAction].canceled -= OnLook;
        PlayerInput.actions[crouchAction].performed -= OnCrouch;
        PlayerInput.actions[crouchAction].canceled -= OnCrouch;
        PlayerInput.actions[sprintAction].performed -= OnSprint;
        PlayerInput.actions[sprintAction].canceled -= OnSprint;

    }

    public void InMenuExit()
    {
        PlayerInput.actions[moveAction].performed += OnMove;
        PlayerInput.actions[moveAction].canceled += OnMove;
        PlayerInput.actions[lookAction].performed += OnLook;
        PlayerInput.actions[lookAction].canceled += OnLook;
        PlayerInput.actions[crouchAction].performed += OnCrouch;
        PlayerInput.actions[crouchAction].canceled += OnCrouch;
        PlayerInput.actions[sprintAction].performed += OnSprint;
        PlayerInput.actions[sprintAction].canceled += OnSprint;

    }

    #endregion

    // DEBUG
    private void Update()
    {
        currentActionMapName = PlayerInput.currentActionMap.name;
    }

}
