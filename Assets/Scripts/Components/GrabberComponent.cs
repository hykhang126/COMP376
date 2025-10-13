using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(CapsuleCollider))]
public class GrabberComponent : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField]
    private String _interactionAction = "Grab";
    private String interactionAction
    {
        set
        {
            playerInput.actions[_interactionAction].performed -= OnGrabTriggered;
            _interactionAction = value;
            playerInput.actions[_interactionAction].performed += OnGrabTriggered;
        }
        get
        {
            return _interactionAction;
        }
    }

    [Header("Forward probe (local Z)")]
    [SerializeField]
    float length = 2f;       // how far forward the probe reaches
    [SerializeField]
    float radius = 0.05f;    // how thin the probe is

    [SerializeField, HideInInspector]
    private CapsuleCollider probe;
    private GameObject carryableObject = null;

    void Awake()
    {
        if (playerInput == null)
        {
            Debug.LogError("Must assign playerInput in InteractorCOmponent!");
            return;
        }
        else
        {
            playerInput.actions[_interactionAction].performed += OnGrabTriggered;
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

    }

    private void OnGrabTriggered(InputAction.CallbackContext context)
    {
        if (carryableObject != null){
            Player.InstanceReference.AttemptGrabItem(carryableObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            carryableObject = other.gameObject;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        carryableObject = null;
    }

    void OnValidate()
    {
        reset_probe();
    }
}
