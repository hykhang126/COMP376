using System.Linq;
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

    private Material interact_material;

    void Start()
    {
        interact_material = Resources.Load<Material>("Materials/interact_glow");
        if(interact_material == null)
        {
            Debug.LogError("Could not load interact_glow.mat. It must be inside a Resources/ folder!");
        }
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
        if (_interactable_component != null)
        {
            var renderer = _interactable_component.GetComponentInChildren<Renderer>();
            RemoveGlow(renderer, interact_material);
            _interactable_component.AttempyTriggerInteraction();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<InteractableComponent>() != null)
        {
            _interactable_component = other.gameObject.GetComponent<InteractableComponent>();
            if (_interactable_component != null && !_interactable_component.isCoolingDown)
            {
                canInteractorTrigger = true;
                _interactable_component.interactionEntered.Invoke();
                var renderer = other.GetComponentInChildren<Renderer>();
                AddGlow(renderer, interact_material);
            }
        }
    }

   void OnTriggerStay(Collider other)
    {
        var interactable = other.GetComponentInParent<InteractableComponent>();
        
        if (interactable == null || interactable != _interactable_component)
            return;
        if (_interactable_component.isCoolingDown) 
            return;

        var renderer = other.GetComponentInChildren<Renderer>();
        AddGlow(renderer, interact_material); // will no-op if already present
    }

    
    void OnTriggerExit(Collider other)
    {
        _interactable_component = other.gameObject.GetComponent<InteractableComponent>();
        if (_interactable_component != null)
        {
            _interactable_component.interactionExited.Invoke();
            
            var renderer = _interactable_component.GetComponentInChildren<Renderer>();
            RemoveGlow(renderer, interact_material);

            canInteractorTrigger = false;
            _interactable_component = null;
            
        }
    }

    bool AddGlow(Renderer r, Material glow)
    {
        if (!r || !glow) return false;
        var mats = r.sharedMaterials.ToList();
        if (!mats.Contains(glow))
        {
            mats.Add(glow);
            r.sharedMaterials = mats.ToArray();
            return true;
        }
        return false;
    }

    bool RemoveGlow(Renderer r, Material glow)
    {
        if (!r || !glow) return false;
        var mats = r.sharedMaterials.ToList();
        if (mats.Contains(glow))
        {
            mats.Remove(glow);
            r.sharedMaterials = mats.ToArray();
            return true;
        }
        return false;
    }

    void OnValidate()
    {
        Reset_probe();
    }

    #region  SUSSY callbacks
    public void OnSussyInteractableDestroyed(bool interactable)
    {
        canInteractorTrigger = interactable;
    }
    #endregion
}
