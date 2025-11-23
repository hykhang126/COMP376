using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteractable : Interactable
{
    [Header("Key Settings")]
    [SerializeField] private ItemContractSO[] itemContractSOsToOpenThisDoor;
    [SerializeField] private bool isLockedByKeys = false;

    private Door door;

    //private AsyncOperation loadNextScene;

    [SerializeField] private ItemKeyToSceneNameSO itemKeyToSceneNameSO;
    private Dictionary<string, string> itemKeyToSceneName;

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
        itemKeyToSceneName = itemKeyToSceneNameSO.itemKeyToSceneName;
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
            else if (Player.InstanceReference.inventory)
            {
                for (int i = 0; i < itemContractSOsToOpenThisDoor.Length; i++)
                {
                    //----- To be replaced by getting the equipped key and checking if it is in the array -----
                    if (Player.InstanceReference.inventory.items[Player.InstanceReference.inventory.GetCurrentItemIndex()].Id == itemContractSOsToOpenThisDoor[i].Id)
                    {
                        OpenDoor();
                        Player.InstanceReference.inventory.RemoveItem();
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

    /*private void LoadNextScene(Player player, int keyIndex)
    {
        loadNextScene = SceneManager.LoadSceneAsync(itemKeyToSceneName[itemContractSOsToOpenThisDoor[keyIndex]], LoadSceneMode.Additive);
        player.inventory.RemoveItemAtIndex(player.inventory.GetCurrentItemIndex());
        loadNextScene.completed += UnlockDoor;
    }*/

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
