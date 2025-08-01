using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteractable : Interactable
{
    
    [SerializeField] private int[] itemKeysToOpenThisDoor;

    private AsyncOperation loadNextScene;

    private Dictionary<int, string> itemKeyToSceneName = new Dictionary<int, string>
    {
        // Insert the rest of the key item keys here alongside the scene name they are supposed to open
        {101, "Scene 1"}
    };
    //[SerializeField] private Texture2D inventoryImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Item created with key: " + itemKeysToOpenThisDoor);
    }

    public override void Interact(Player player)
    {
        if (player)
        {
            if (player.inventory)
            {
                for (int i = 0; i < itemKeysToOpenThisDoor.Length; i++)
                {
                    //----- To be replaced by getting the equipped key and checking if it is in the array -----
                    if (player.inventory.GetEquippedItemKey() == itemKeysToOpenThisDoor[i])
                    {
                        loadNextScene = SceneManager.LoadSceneAsync(itemKeyToSceneName[itemKeysToOpenThisDoor[i]], LoadSceneMode.Additive);
                        player.inventory.RemoveItemAtIndex(player.inventory.GetCurrentItemIndex());
                        loadNextScene.completed += UnlockDoor;
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

    private void UnlockDoor(AsyncOperation op)
    {
        //TODO: Door Opening anim and sound
        Debug.Log("Door Open");
    }
}
