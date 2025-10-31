using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
public class InteractorComponent : MonoBehaviour
{
    [Header("Forward probe (local Z)")]
    [SerializeField]
    float length = 2f;       // how far forward the probe reaches
    [SerializeField]
    float radius = 0.05f;    // how thin the probe is

    [SerializeField, HideInInspector] private CapsuleCollider probe;

    [Header("DEBUG: No assignment")]
    [Tooltip("The currently detected interactable object")]
    [SerializeField] private InteractableComponent _interactable_component;
    [Tooltip("Can the interactor currently trigger interaction?")]
    public bool canInteractorTrigger = false;

    private PlayerInputHandler playerInputHandler;

    void Start()
    {
        // Grab the PlayerInputHandler from Player Instance
        if (Player.InstanceReference != null)
        {
            playerInputHandler = Player.InstanceReference.playerInputHandler;
            if (playerInputHandler != null)
            {
                playerInputHandler.AddActionSubscriber(playerInputHandler.interactionAction, OnInteractTriggered);
            }
            else
            {
                Debug.LogError("PlayerInputHandler component not found on Player instance!");
            }
        }
        else
        {
            Debug.LogError("Player instance not found!");
        }
        
        Reset_probe();
    }


    private void Reset_probe()
    {
        probe = GetComponent<CapsuleCollider>();
        if (probe == null)
        {
            probe = (CapsuleCollider)gameObject.AddComponent(typeof(CapsuleCollider));
        }
        probe.radius = radius;
        probe.height = length;
        probe.direction = 2;
        probe.center = new Vector3(0f, 0f, length / 2f);
        probe.isTrigger = true;
        probe.includeLayers = LayerMask.GetMask("Interact");

    }

    private void OnInteractTriggered(InputAction.CallbackContext context)
    {
        // Check which action was triggered by its name
        if (canInteractorTrigger)
        {
            // Invoke trigger event in InteractableComponent
            _interactable_component.AttempyTriggerInteraction();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<InteractableComponent>() != null && !canInteractorTrigger)
        {
            _interactable_component = other.gameObject.GetComponent<InteractableComponent>();
            if (_interactable_component != null)
            {
                canInteractorTrigger = true;   
                _interactable_component.interactionEntered.Invoke();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        _interactable_component = other.gameObject.GetComponent<InteractableComponent>();
        if (_interactable_component != null && canInteractorTrigger)
        {
            _interactable_component.interactionExited.Invoke();
            _interactable_component = null;
            canInteractorTrigger = false;
        }
    }

    void OnValidate()
    {
        Reset_probe();
    }
}
