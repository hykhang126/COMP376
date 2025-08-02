using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteractable : Interactable
{
    [Header("Key Settings")]
    [SerializeField] private int[] itemKeysToOpenThisDoor;
    [SerializeField] private bool isLockedByKeys = false;

    private Door door;

    private AsyncOperation loadNextScene;

    [SerializeField] private ItemKeyToSceneNameSO itemKeyToSceneNameSO;
    private Dictionary<int, string> itemKeyToSceneName;

    void Awake()
    {
        door = GetComponent<Door>();
        if (door == null)
        {
            Debug.LogError("Door component not found on the DoorInteractable object.");
        }
        
        isLockedByKeys = itemKeysToOpenThisDoor.Length > 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemKeyToSceneName = itemKeyToSceneNameSO.itemKeyToSceneName;
    }

    public override void Interact(Player player)
    {
        // GMTK 25 - We can either teleport the player or just open the door
        if (player)
        {
            // If door is not locked by keys, open it directly
            if (!isLockedByKeys)
            {
                OpenDoor(player);
            }
            else if (player.inventory)
            {
                for (int i = 0; i < itemKeysToOpenThisDoor.Length; i++)
                {
                    //----- To be replaced by getting the equipped key and checking if it is in the array -----
                    if (player.inventory.GetEquippedItemKey() == itemKeysToOpenThisDoor[i])
                    {
                        OpenDoor(player);
                        break;
                        //-------------------------------------------------------------------------------------
                    }
                    else
                    {
                        Debug.Log("Player doesn't have the correct item equipped in their inventory!");
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

    private void LoadNextScene(Player player, int keyIndex)
    {
        loadNextScene = SceneManager.LoadSceneAsync(itemKeyToSceneName[itemKeysToOpenThisDoor[keyIndex]], LoadSceneMode.Additive);
        player.inventory.RemoveItemAtIndex(player.inventory.GetCurrentItemIndex());
        loadNextScene.completed += UnlockDoor;
    }

    private void OpenDoor(Player player)
    {
        if (door.isTeleportable && door.teleportTarget != null)
        {
            player.transform.position = door.teleportTarget.position;
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
