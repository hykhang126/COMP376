using UnityEngine;
using UnityEngine.UI;

public class DoorInteractable : Interactable
{
    
    [SerializeField] private int itemKey;
    //[SerializeField] private Texture2D inventoryImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Item created with key: " + itemKey);
    }

    public override void Interact(Player player)
    {
        if (player)
        {
            if (player.inventory)
            {
                if (player.inventory.UseItem(itemKey))
                {
                    //TODO: Play open door animation
                    //TODO: Load next level
                    Debug.Log("Door Open");
                }
                else
                {
                    Debug.Log("Player doesn't have a key in their inventory!");
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
}
