using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent, RequireComponent(typeof(CapsuleCollider))]
public class InteractorComponent : MonoBehaviour
{

    [SerializeField] private PlayerInput playerInput;

    [Header("Forward probe (local Z)")]
    [SerializeField]
    float length = 2f;       // how far forward the probe reaches
    [SerializeField]
    float radius = 0.05f;    // how thin the probe is

    public bool canInteractorTrigger { get; private set; }

    [SerializeField,HideInInspector]
    private CapsuleCollider probe;
    private InteractableComponent _interactable_component;

    void Awake()
    {
        if (playerInput == null)
        {
            Debug.LogError("Must assign playerInput in InteractorCOmponent!");
            return;
        }
        else
        {
            playerInput.actions["Interact"].performed += OnInteractTriggered;
        }
    }



    void Start()
    {
        
        reset_probe();
    }


    private void reset_probe()
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
        reset_probe();
    }
}
