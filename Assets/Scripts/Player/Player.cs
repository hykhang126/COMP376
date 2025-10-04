using UnityEngine;
using System;
using System.Collections;
using NaughtyAttributes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    // Global Player reference
    public static Player InstanceReference { get; private set; }

    Vector2 movementInput;

    [SerializeField] float movementSpeed = 2.5f;
    [SerializeField] float sprintingSpeed = 4f;
    [SerializeField][Range(0f, 1f)] float sprintAcceleration = 0.15f;

    [SerializeField] float crouchSpeed = 1.5f;
    [SerializeField] bool isInverted = false;

    GameObject _camera;

    Rigidbody rb;

    // Clamp angles for vertical look (Y-axis rotation)
    [SerializeField] private float minY = -40f;  // Min vertical rotation angle
    [SerializeField] private float maxY = 40f;   // Max vertical rotation angle

    [SerializeField][Range(0f, 1f)] private float cameraSmoothing = 0.5f;

    private float currentPitch = 0f;  // Track current pitch (vertical rotation)
    private float currentYaw = 0f;

    //Set sensitivity whenever inputMode is updated
    [SerializeField]
    private INPUT_MODE _inputMode = INPUT_MODE.MOUSE_KEYBOARD;
    public INPUT_MODE InputMode
    {
        get { return _inputMode; }
        set
        {
            _inputMode = value;
            if (value == INPUT_MODE.GAMEPAD)
            {
                sensitivity = gamepadSensitivityMultiplier;
            }
            else
            {
                sensitivity = mouseSensitivityMultiplier;
            }
        }
    }
    public enum INPUT_MODE { MOUSE_KEYBOARD, GAMEPAD };

    [SerializeField] private float mouseSensitivityMultiplier = 5f; // Mouse sensitivity multiplier

    [SerializeField] private float gamepadSensitivityMultiplier = 5f; // Gamepad analog stick sensitivity multiplier

    private float sensitivity = 5f;

    private Vector2 lookInput;

    public StateMachine stateMachine { get; private set; }

    public PlayerInput playerInput { get; private set; }

    public Inventory inventory { get; private set; }

    public bool ToggleRotation { get; private set; } = false;

    public GameObject carriedObject;

    public GameObject carryPoint;

    [SerializeField] private float hitRange = 2f;

    [SerializeField] private HUD HUD;

    public AudioSource playerAudioSource;

    [SerializeField] private GameSettingsSO gameSettingsSO;

    public void Awake()
    {

        playerInput = GetComponent<PlayerInput>();
        stateMachine = GetComponent<StateMachine>();

        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled += OnLook;
        playerInput.actions["Sprint"].performed += OnSprint;
        playerInput.actions["Sprint"].canceled += OnSprint;
        playerInput.actions["Crouch"].performed += OnCrouch;
        playerInput.actions["Crouch"].canceled += OnCrouch;


        playerInput.actions["RotateCarryObject"].performed += ctx => RotateCarryObject();

        // Set payer instance reference on init and remove any old refrences
        if (InstanceReference != null && InstanceReference != this)
        {
            // Makes sure no duplicate instances can exsit
            Debug.LogError($"Destroying duplicate Player instance '{gameObject}'");
            Destroy(gameObject);
            return;
        }
        InstanceReference = this;
        // Keep object loaded between scene loads. Not required 
        //DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        // Set currentYaw value to starting transform forward direction
        currentYaw = transform.rotation.y;
        // Set starting sensitivity based on initial _inputMode
        if (_inputMode == INPUT_MODE.MOUSE_KEYBOARD)
        {
            sensitivity = mouseSensitivityMultiplier;
        }
        else
        {
            sensitivity = gamepadSensitivityMultiplier;
        }

        // Connect to HUD
        if (HUD == null)
        {
            Debug.LogError("HUD not found");
        }
        Cursor.lockState = CursorLockMode.Locked;
        _camera = GameObject.Find("camera");
        rb = GetComponent<Rigidbody>();

        // Initialize camera rotation
        _camera.transform.localRotation = Quaternion.Euler(0, 0, 0);

        //Connect to Inventory System
        inventory = FindAnyObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory System not found in Hierarchy");
        }

        //Intialize carry point
        carryPoint = new GameObject("CarryPoint");
        carryPoint.transform.SetParent(_camera.transform);
        carryPoint.transform.localPosition = new Vector3(0, 0, 2f);

        // Add Rigidbody to carryPoint so it can act as a joint anchor
        Rigidbody carryRb = carryPoint.AddComponent<Rigidbody>();
        carryRb.useGravity = false;
        carryRb.isKinematic = true;

        // Audio Source
        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
        {
            playerAudioSource = gameObject.AddComponent<AudioSource>();
            playerAudioSource.playOnAwake = false;
        }

        // Game Settings
        if (!gameSettingsSO)
        {
            gameSettingsSO = Resources.Load<GameSettingsSO>("Scriptable Objects/GameSettingsSO");
        }
    }

    public void RotateCarryObject()
    {
        if (Player.InstanceReference.stateMachine.GetCurrentStateName() == PlayerStateType.CarryingObject.ToString())
        {
            Debug.Log("Player is rotating the carried object.");
            stateMachine.InvokeStateEvent(PlayerStateType.RotatingCarryObject.ToString());
            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Look"].performed -= OnLook;
            playerInput.actions["Look"].performed += carriedObject.GetComponent<CarryInteractable>().RotateObject;
            carriedObject.GetComponent<CarryInteractable>().DisableFixedJoint();
        }
        else if (Player.InstanceReference.stateMachine.GetCurrentStateName() == PlayerStateType.RotatingCarryObject.ToString())
        {
            Debug.Log("Player is not rotating the carried object.");
            stateMachine.InvokeStateEvent(PlayerStateType.CarryingObject.ToString());
            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Look"].performed -= carriedObject.GetComponent<CarryInteractable>().RotateObject;
            playerInput.actions["Look"].performed += OnLook;
            carriedObject.GetComponent<CarryInteractable>().EnableFixedJoint();
        }
        else
        {
            Debug.Log("Player is not carrying an object to rotate.");
        }
    }

    public void SetIsCarrying(bool result)
    {
        if (result)
        {
            Player.InstanceReference.stateMachine.InvokeStateEvent(PlayerStateType.CarryingObject.ToString());
        }
        else
        {
            Player.InstanceReference.stateMachine.InvokeStateEvent(PlayerStateType.Idle.ToString());
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        if (!isInverted)
        {
            //Invert to adjust for inverted camera input
            lookInput.y *= -1f;
        }
        currentYaw += lookInput.x * sensitivity * Time.deltaTime;
        currentPitch += lookInput.y * sensitivity * Time.deltaTime;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        bool isSprinting = context.ReadValueAsButton();
        if (isSprinting && movementInput.y > float.Epsilon) // Only sprint if forward move input is held
            stateMachine.InvokeStateEvent("toSprinting");
        else
        {
            stateMachine.InvokeStateEvent("toIdle");
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        bool isCrouching = context.ReadValueAsButton();
        if (isCrouching)
        {
            stateMachine.InvokeStateEvent("toCrouch");
        }
        else
        {
            stateMachine.InvokeStateEvent("toIdle");
        }
    }

    #region Idle State Callbacks
    public void IdleUpdate()
    {
        // Clamp pitch camera (local)
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // Rotate camera
        Quaternion curretnPitchRotation = _camera.transform.localRotation;
        Quaternion tragetPitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        // Slerp from current pitch to target pitch using cameraSmoothing
        _camera.transform.localRotation = Quaternion.Slerp(curretnPitchRotation, tragetPitchRotation, 1f - cameraSmoothing);

    }

    public void IdleFixedUpdate()
    {
        // Rotate Body
        Quaternion currentBodyRotation = rb.transform.rotation;
        Quaternion targetBodyRotation = Quaternion.Euler(0f, currentYaw, 0f);
        // Slerp from current yaw yaw to target yaw using cameraSmoothing
        rb.transform.rotation = Quaternion.Slerp(currentBodyRotation, targetBodyRotation, 1f - cameraSmoothing);

        // Move player
        Vector3 moveDelta = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        Vector3 velocity = transform.TransformDirection(moveDelta) * movementSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

    }
    #endregion
    #region Sprinting State Callbacks

    public void SprintingUpdate()
    {
        // Clamp pitch camera (local)
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // Rotate camera
        Quaternion curretnPitchRotation = _camera.transform.localRotation;
        Quaternion tragetPitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        // Slerp from current pitch to target pitch using cameraSmoothing
        _camera.transform.localRotation = Quaternion.Slerp(curretnPitchRotation, tragetPitchRotation, 1f - cameraSmoothing);
    }

    public void SprintingFixedUpdate()
    {
        // Return to Idle state if player is not moving or has stopped pressing movemnet input
        if (math.abs(movementInput.magnitude) <= float.Epsilon || math.abs(rb.linearVelocity.magnitude) <= float.Epsilon)
        {
            stateMachine.InvokeStateEvent("toIdle");
        }

        // Rotate Body
        Quaternion currentBodyRotation = rb.transform.rotation;
        Quaternion targetBodyRotation = Quaternion.Euler(0f, currentYaw, 0f);
        // Slerp from current yaw yaw to target yaw using cameraSmoothing
        rb.transform.rotation = Quaternion.Slerp(currentBodyRotation, targetBodyRotation, 1f - cameraSmoothing);

        // Move player
        Vector3 moveDelta = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = transform.TransformDirection(moveDelta) * sprintingSpeed;
        // Lerp to accelerate to sprinting speed
        Vector3 velocity = Vector3.Lerp(currentVelocity, targetVelocity, sprintAcceleration);
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }
    #endregion
    #region Crouch State Callbacks

    public void CrouchEnter()
    {
        // Stop other coroutine if currently playing
        //StopCoroutine(LerpToStandingScale());
        // Lerp to crouching scale
        //StartCoroutine(LerpToCrouchScale());
    }

    public void CrouchExit()
    {
        // Stop other coroutine if currently playing
        //StopCoroutine(LerpToCrouchScale());
        // Lerp to Standing scale
        StartCoroutine(LerpToStandingScale());
    }

    public void CrouchUpdate()
    {
        // Clamp pitch camera (local)
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // Rotate camera
        Quaternion curretnPitchRotation = _camera.transform.localRotation;
        Quaternion tragetPitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        // Slerp from current pitch to target pitch using cameraSmoothing
        _camera.transform.localRotation = Quaternion.Slerp(curretnPitchRotation, tragetPitchRotation, 1f - cameraSmoothing);
    }

    public void CrouchFixedUpdate()
    {
        //Lerp to crouch scale over 4 frames
        LerpToCrouchScale();
        // Rotate Body
        Quaternion currentBodyRotation = rb.transform.rotation;
        Quaternion targetBodyRotation = Quaternion.Euler(0f, currentYaw, 0f);
        // Slerp from current yaw yaw to target yaw using cameraSmoothing
        rb.transform.rotation = Quaternion.Slerp(currentBodyRotation, targetBodyRotation, 1f - cameraSmoothing);

        // Move player
        Vector3 moveDelta = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        Vector3 velocity = transform.TransformDirection(moveDelta) * crouchSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    void LerpToCrouchScale()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1f, 0.5f, 1f), 0.25f);
    }

    IEnumerator LerpToStandingScale()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(1f, 1f, 1f);
        float timeElapsed = 0f;
        float lerpDuration = 0.15f;

        while (timeElapsed < lerpDuration)
        {
            float t = timeElapsed / lerpDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            timeElapsed += Time.fixedDeltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
    #endregion
    #region InMenu State Callbacks
    public void InMenuEnter()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Look"].performed -= OnLook;
        playerInput.actions["Look"].canceled -= OnLook;
        playerInput.actions["Crouch"].performed -= OnCrouch;
        playerInput.actions["Crouch"].canceled -= OnCrouch;
        playerInput.actions["Sprint"].performed -= OnSprint;
        playerInput.actions["Sprint"].canceled -= OnSprint;

    }

    public void InMenuExit()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled += OnLook;
        playerInput.actions["Crouch"].performed += OnCrouch;
        playerInput.actions["Crouch"].canceled += OnCrouch;
        playerInput.actions["Sprint"].performed += OnSprint;
        playerInput.actions["Sprint"].canceled += OnSprint;

    }

    #endregion

}
