using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("Key Settings")]
    [SerializeField] private ItemContractSO[] itemContractSOsToOpenThisDoor;
    [SerializeField] private bool isLockedByKeys = false;

    private Door door;
    private DoorAction doorAction;

    [SerializeField] private AudioClip lockedSound;

    void Awake()
    {
        door = GetComponent<Door>();
        if (door == null)
        {
            Debug.LogError("Door component not found on the DoorInteractable object.");
        }
        
        isLockedByKeys = itemContractSOsToOpenThisDoor.Length > 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        doorAction = gameObject.GetComponent<DoorAction>();
    }

    public override void Interact()
    {
        // GMTK 25 - We can either teleport the player or just open the door
        if (Player.InstanceReference)
        {
            // If door is not locked by keys, open it directly
            if (!isLockedByKeys)
            {
                OpenDoor();
            }
            else if (Inventory.InstanceReference != null)
            {
                for (int i = 0; i < itemContractSOsToOpenThisDoor.Length; i++)
                {
                    //----- To be replaced by getting the equipped key and checking if it is in the array -----
                    if (Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Id == itemContractSOsToOpenThisDoor[i].Id)
                    {
                        OpenDoor();
                        Inventory.InstanceReference.RemoveItem();
                        break;
                        //-------------------------------------------------------------------------------------
                    }
                    else
                    {
                        doorAction.doorAudioSource.PlayOneShot(lockedSound);
                    }
                }
            }
            else
            {
                Debug.LogError("Player is null, cannot interact with item.");
            }
        }
        else
        {
            Debug.LogError("Player's inventory is null, cannot add item.");
            return;
        }
    }

    private void OpenDoor()
    {
        if (door.isTeleportable && door.teleportTarget != null)
        {
            Player.InstanceReference.transform.position = door.teleportTarget.position;
            Debug.Log("Player teleported to " + door.teleportTarget.name);
        }
        else if (door.isInteractable && door.doorInteractable != null)
        {
            if (door.CheckIfDoorIsOpened())
            {
                door.CloseDoorInspector();
            }
            else
            {
                door.OpenDoorInspector();
            }
        }
        else
        {
            Debug.LogError("Door is not interactable or teleportable.");
        }
    }

    private void UnlockDoor(AsyncOperation op)
    {
        // TODO: Implement the logic to unlock the door on successful scene load
        Debug.Log("Door Unlocked and opened.");
    }
}
