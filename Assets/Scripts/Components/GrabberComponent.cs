using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
public class GrabberComponent : MonoBehaviour
{
    [Header("Forward probe (local Z)")]
    [SerializeField]
    float length = 2f;       // how far forward the probe reaches
    [SerializeField]
    float radius = 0.05f;    // how thin the probe is

    [SerializeField, HideInInspector] private CapsuleCollider probe;

    [Header("DEBUG: No assignment")]
    [Tooltip("The currently detected carryable object")]
    [SerializeField] private GameObject carryableObject = null;

    private PlayerInputHandler playerInputHandler;

    void Start()
    {
        // Grab the PlayerInputHandler from Player Instance
        if (Player.InstanceReference != null)
        {
            playerInputHandler = Player.InstanceReference.playerInputHandler;
            if (playerInputHandler != null)
            {
                playerInputHandler.AddActionSubscriber(playerInputHandler.grabAction, OnGrabTriggered);
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

    }

    private void OnGrabTriggered(InputAction.CallbackContext context)
    {
        if (carryableObject != null)
        {
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
        Reset_probe();
    }
}
