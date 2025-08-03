using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    Vector2 movementInput;

    [SerializeField] float movementSpeed = 5f;
    [SerializeField] bool isInverted = false;

    GameObject _camera;

    private float lookDeltaX = 0f;
    private float lookDeltaY = 0f;

    Rigidbody rb;

    // Clamp angles for vertical look (Y-axis rotation)
    [SerializeField] private float minY = -40f;  // Min vertical rotation angle
    [SerializeField] private float maxY = 40f;   // Max vertical rotation angle

    private float currentXRotation = 0f;  // Track current pitch (vertical rotation)
    private float targetYRotation = 0f;   // Track current yaw (horizontal rotation)

    [SerializeField] private float mouseSensitivity = 100f; // Mouse sensitivity multiplier

    [SerializeField] private float gamepadSensitivity = 5f; // Gamepad analog stick sensitivity multiplier

    private Vector2 lookInput;

    public PlayerInput playerInput { get; private set; }

    private RaycastHit hit;

    public Inventory inventory { get; private set; }

    public bool ToggleRotation { get; private set; } = false;

    public GameObject carriedObject;

    public GameObject carryPoint;

    [SerializeField] private float hitRange = 2f;

    private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepInterval = 0.5f; // Adjust based on desired pacing

    private Vector3 lastPosition;

    private float footstepTimer = 0f;

    private float continuousMovementTime = 0f;
    [SerializeField] private float minMovementDuration = 0.2f; // Require 0.1 seconds of movement before footsteps




    HUD HUD;

    public void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions.FindActionMap("Player").Disable();

    }

    void Start()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled += OnLook;
        //playerInput.actions["Scroll"].performed += inventory.CycleItems;

        playerInput.actions["Interact"].performed += ctx => OnInteract();
        playerInput.actions["RotateCarryObject"].performed += ctx => RotateCarryObject();


        HUD = GameObject.Find("UI").transform.Find("HUD").GetComponent<HUD>();
        if (HUD == null)
        {
            Debug.LogError("HUD not found");
        }
        Cursor.lockState = CursorLockMode.Locked;
        _camera = GameObject.Find("camera");
        rb = GetComponent<Rigidbody>();

        // Disable Rigidbody rotation so manual rotation doesn't conflict
        rb.freezeRotation = true;

        // Initialize camera rotation
        _camera.transform.localRotation = Quaternion.Euler(0, 0, 0);

        hit = new RaycastHit();

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

        //Add footstep sounds
        footstepAudioSource = gameObject.GetComponent<AudioSource>();

    }

    public void RotateCarryObject()
    {
        if (PlayerState.instance.currentState == PlayerStateType.CarryingObject)
        {
            Debug.Log("Player is rotating the carried object.");
            PlayerState.instance.TriggerTransition(PlayerStateType.RotatingCarryObject);
            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Look"].performed -= OnLook;
            playerInput.actions["Look"].performed += carriedObject.GetComponent<CarryInteractable>().RotateObject;
            carriedObject.GetComponent<CarryInteractable>().DisableFixedJoint();
        }
        else if (PlayerState.instance.currentState == PlayerStateType.RotatingCarryObject)
        {
            Debug.Log("Player is not rotating the carried object.");
            PlayerState.instance.TriggerTransition(PlayerStateType.CarryingObject);
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
            PlayerState.instance.TriggerTransition(PlayerStateType.CarryingObject);
        }
        else
        {
            PlayerState.instance.TriggerTransition(PlayerStateType.Idle);
        }
    }

    public RaycastHit GetRaycastHit()
    {
        return hit;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (PlayerState.instance.currentState == PlayerStateType.InMenu) return;
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (PlayerState.instance.currentState == PlayerStateType.InMenu) return;
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnInteract()
    {
        if (PlayerState.instance.currentState == PlayerStateType.InMenu) return;
        Debug.Log("Interact button pressed");
        if (carriedObject != null && (PlayerState.instance.currentState == PlayerStateType.CarryingObject || PlayerState.instance.currentState == PlayerStateType.RotatingCarryObject))
        {
            // If the player is carrying an object, drop it
            carriedObject.GetComponent<CarryInteractable>().Interact(this);
            return;
        }
        if (hit.collider != null && hit.collider.gameObject.GetComponent<Interactable>() != null)
        {
            // Call the Interact method on the Interactable component
            hit.collider.GetComponent<Interactable>().Interact(this);
        }
    }

    void Update()
    {
        // Draw a ray for debugging purposes
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * hitRange, Color.red);
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, hitRange))
        {
            // Pierce through the player layer to allow interaction
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                hit = new RaycastHit();
            }
            //Check if the object has an Interactable component, show UI prompt to tell the player they can interact.
            if (hit.collider != null && hit.collider.gameObject.GetComponent<Interactable>() != null)
            {
                HUD.ShowInteractPrompt(true);
                //Debug.Log("Press 'E' to interact");
            }
        }
        else
        {
            HUD.ShowInteractPrompt(false);
        }

        // Movement
        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        targetYRotation += lookDeltaX;

        //Footstep logic
        // Footstep sound logic
        // Check actual movement (to avoid false positives from short taps)

        bool isMoving = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;

        if (isMoving)
        {
            continuousMovementTime += Time.deltaTime;

            if (continuousMovementTime >= minMovementDuration)
            {
                footstepTimer += Time.deltaTime;

                if (footstepTimer >= footstepInterval)
                {
                    PlayFootstepSound();
                    footstepTimer = 0f;
                }
            }
        }
        else
        {
            continuousMovementTime = 0f;
            footstepTimer = footstepInterval; // Ready to play again when movement resumes
        }


        lastPosition = transform.position;

    }

    private void PlayFootstepSound()
    {
        if (footstepClip != null && footstepAudioSource != null)
        {
            footstepAudioSource.pitch = Random.Range(0.95f, 1.05f);
            footstepAudioSource.PlayOneShot(footstepClip);
        }
    }


    void FixedUpdate()
    {
        // Movement
        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        Vector3 velocity = transform.TransformDirection(moveDirection) * movementSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }


    void LateUpdate()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            lookDeltaX += lookInput.x * gamepadSensitivity * Time.deltaTime;
            lookDeltaY += lookInput.y * gamepadSensitivity * Time.deltaTime;
        }
        else if (Gamepad.current == null)
        {
            lookDeltaX += lookInput.x * mouseSensitivity * Time.deltaTime;
            lookDeltaY += lookInput.y * mouseSensitivity * Time.deltaTime;
        }

        // Horizontal rotation (player)
        transform.Rotate(Vector3.up * lookDeltaX); // Apply horizontal rotation here!

        // Vertical rotation (camera)
        if (!isInverted)
            currentXRotation -= lookDeltaY;
        else
            currentXRotation += lookDeltaY;

        currentXRotation = Mathf.Clamp(currentXRotation, minY, maxY);
        _camera.transform.localRotation = Quaternion.Euler(currentXRotation, 0, 0);

        // Reset deltas
        lookDeltaX = 0f;
        lookDeltaY = 0f;
    }


}
