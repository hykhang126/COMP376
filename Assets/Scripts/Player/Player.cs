using UnityEngine;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.VisualScripting;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    // Global Player reference
    public static Player InstanceReference { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public Inventory inventory { get; private set; }
    private Vector2 lookInput;
    GameObject _camera;
    Rigidbody rb;
    // ref to currently carried item
    [DoNotSerialize]
    GameObject carriedItem = null;
    Vector2 movementInput;


    [SerializeField] GameObject carryAnchor;

    [Header("Idle State")]
    [SerializeField] float movementSpeed = 2.5f;
    [Header("Sprinting State")]
    [SerializeField] float sprintingSpeed = 4f;
    [SerializeField][Range(0f, 1f)] float sprintAcceleration = 0.15f;
    [Header("Crouch State")]
    [SerializeField] float crouchSpeed = 1.5f;
    [Header("Camera")]
    [SerializeField] bool isInverted = false;
    // Clamp angles for vertical look (Y-axis rotation)
    [SerializeField] private float minY = -40f;  // Min vertical rotation angle
    [SerializeField] private float maxY = 40f;   // Max vertical rotation angle
    [SerializeField][Range(0f, 1f)] private float cameraSmoothing = 0.5f;
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

    private float currentPitch = 0f;  // Track current pitch (vertical rotation)
    private float currentYaw = 0f;
    private float sensitivity = 5f;

    [Header("HUD")]

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

    void Update()
    {
        
        if (carriedItem != null)
        {
            Vector3 itemPos = carriedItem.transform.position;
            Vector3 anchorPos = carryAnchor.transform.position;
            float dist = Vector3.Distance(itemPos, anchorPos)*5.0f;
            Vector3 targetPos = Vector3.Lerp(itemPos,anchorPos,Time.fixedDeltaTime*dist);
            carriedItem.GetComponent<Rigidbody>().MovePosition(targetPos);
        }   
    }

    #region  Item Carrying

    public void AttemptGrabItem(GameObject item)
    {
        if (IsHoldingItem() == false)
        {
            carriedItem = item;
            playerInput.actions["Grab"].performed += OnGrab;
            carriedItem.GetComponent<Rigidbody>().useGravity = false;
            carriedItem.GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    public void DropItem()
    {
        if (IsHoldingItem())
        {
            playerInput.actions["Grab"].performed -= OnGrab;
            carriedItem.GetComponent<Rigidbody>().useGravity = true;
            carriedItem.GetComponent<Rigidbody>().freezeRotation = false;
            carriedItem = null; 
        }
    }
    public bool IsHoldingItem()
    {
        return carriedItem != null;
    }
    #endregion

    #region Inputs
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

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (IsHoldingItem())
        {
            DropItem();
        }
    }

    #endregion

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

    public void CrouchExit()
    {
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
