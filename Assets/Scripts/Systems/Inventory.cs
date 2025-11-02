using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    // Global Variables
    public List<ItemContractSO> items = new List<ItemContractSO>();
    public GameObject inventoryUI; // Reference to the inventory UI GameObject
    public GameObject itemPreviewPlaceholder;
    public ItemRecipeBook recipeBook;
    public Camera inventoryCamera { get; private set; }

    // Serialized
    [SerializeField] private PlayerInventorySO playerInventorySO;
    [SerializeField] private AudioClip pickUpAudioClip;
    [SerializeField] private int itemFromIndex = -1;
    [SerializeField] private int itemToIndex = -1;

    // Private
    private PlayerInputHandler playerInputHandler;
    private int currentItemIndex = 0;
    private bool isInventoryOpen = false;
    private TextMeshProUGUI itemNameText;
    private ItemContractSO itemFrom;
    private ItemContractSO itemTo;
    private string previousPlayerState;
    private string playerMapName;
    private string inventoryMapName;

    public void Start()
    {
        if (Player.InstanceReference != null)
        {
            playerInputHandler = Player.InstanceReference.playerInputHandler;
            if (playerInputHandler == null)
            {
                Debug.LogError("PlayerInputHandler is null on Player Instance Reference.");
            }
        }
        else
        {
            Debug.LogError("Player Instance Reference is null. Cannot access PlayerInputHandler.");
        }
        playerMapName = playerInputHandler.playerActionMap;
        inventoryMapName = playerInputHandler.inventoryActionMap;

        playerInputHandler.AddMapActionNoParamSubscriber(playerMapName, "InventoryToggle", ToggleInventory);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "InventoryToggle", CloseInventory);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "Next", Next);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "Previous", Previous);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "Combine", Combine);
        playerInputHandler.AddMapActionSubscriber(inventoryMapName, "CycleItems", CycleItems);

        Transform InventoryPanelTransform = inventoryUI.transform.Find("Panel");
        GameObject panel = InventoryPanelTransform != null ? InventoryPanelTransform.gameObject : null;
        Transform itemNameTransform = panel != null ? panel.transform.Find("ItemName") : null;
        itemNameText = itemNameTransform != null ? itemNameTransform.GetComponent<TextMeshProUGUI>() : null;
        inventoryUI.SetActive(false);

#if UNITY_EDITOR
        playerInventorySO.ClearItemsInstance();
#endif
        // Load info from PlayerInventorySO
        if (playerInventorySO != null)
        {
            items = playerInventorySO.items;
            currentItemIndex = playerInventorySO.currentItemIndex;
        }
        else
        {
            Debug.LogError("PlayerInventorySO not found in Resources");
        }

        itemPreviewPlaceholder = gameObject.transform.Find("ItemPreviewPlaceholder").gameObject;
        if(itemPreviewPlaceholder == null)
        {
            Debug.LogError("Did not find Item Preview Placeholder");
        }

    }

    public void ToggleInventory()
    {
        //Logic to toggle the inventory UI
        //unlock the cursor
        Debug.Log("Inventory toggled");
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }

    }

    private void OpenInventory()
    {
        if (Player.InstanceReference.stateMachine.GetCurrentStateName() == PlayerStateType.InMenu.ToString()) return;
        previousPlayerState = Player.InstanceReference.stateMachine.GetCurrentStateName();
        isInventoryOpen = true;
        // Show the inventory UI
        inventoryUI.SetActive(true);
        Cursor.visible = true; // Make the cursor visible
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor

        // Switch to Inventory input map
        playerInputHandler.SwitchInputMap(inventoryMapName);

        //Set the item name text to the last item seen before closing the inventory
        if (items.Count > 0)
        {
            itemNameText.text = items[currentItemIndex].Name; // Update the item name text
        }
        else
        {
            itemNameText.text = ""; // Default text if no items
        }
        /*if (pauseSystem != null)
        {
            pauseSystem.action.Disable();
        }*/

        ItemPreview();
    }

    private void ItemPreview()
    {
        if (items.Count == 0)
        {
            itemPreviewPlaceholder.SetActive(false);
            return;
        }
        else
        {
            itemPreviewPlaceholder.SetActive(true);
        }
        
        itemPreviewPlaceholder.GetComponent<MeshFilter>().mesh = items[currentItemIndex].MeshRef;
    }

    private void CloseInventory()
    {
        isInventoryOpen = false;

        inventoryUI.SetActive(false);
        Cursor.visible = false; // Hide the cursor
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        /*if (player != null)
        {
            player.playerInput.actions.Enable(); // Re-enable player input actions
            pauseSystem.action.Enable();
        }*/
        Player.InstanceReference.playerInputHandler.SwitchInputMap("Player");
    }

    public void CycleItems(InputAction.CallbackContext context)
    {
        if (!isInventoryOpen) return; // Only cycle items if the inventory is open
        int direction = Mathf.RoundToInt(context.ReadValue<Vector2>().y);
        if (items.Count != 0) currentItemIndex = (currentItemIndex + direction + items.Count) % items.Count;

        // Logic to cycle through items in the inventory
        if (items.Count > 0)
        {
            itemNameText.text = items[currentItemIndex].Name; // Update the item name text
        }

        Debug.Log("Current item index after cycling: " + currentItemIndex);

        ItemPreview();
    }
    public void SetCurrentItemIndex(int index)
    {
        currentItemIndex = index;
        Debug.Log("Current item index set to: " + currentItemIndex);
    }

    public int GetCurrentItemIndex()
    {
        return currentItemIndex;
    }

    public void Next()
    {
        if (items.Count == 0 || !isInventoryOpen) return; // No items to cycle through
        currentItemIndex = (currentItemIndex + 1) % items.Count;
        playerInventorySO.currentItemIndex = currentItemIndex; // Update the current item index in the SO
        itemNameText.text = items[currentItemIndex].Name; // Update the item name text

        ItemPreview();
    }

    public void Previous()
    {
        if (items.Count == 0 || !isInventoryOpen) return; // No items to cycle through
        currentItemIndex = (currentItemIndex - 1 + items.Count) % items.Count;
        playerInventorySO.currentItemIndex = currentItemIndex; // Update the current item index in the SO
        itemNameText.text = items[currentItemIndex].Name; // Update the item name text

        ItemPreview();
    }

    public void AddItem(ItemContractSO item)
    {

        if (playerInventorySO != null)
        {
            playerInventorySO.items.Add(item);
            playerInventorySO.currentItemIndex = currentItemIndex;
            if (Player.InstanceReference != null && Player.InstanceReference.playerAudioSource != null)
            {
                Player.InstanceReference.playerAudioSource.pitch = Random.Range(0.9f, 1.1f);
                Player.InstanceReference.playerAudioSource.PlayOneShot(pickUpAudioClip);
            }
        }
        else
        {
            Debug.LogWarning("PlayerInventorySO is null, cannot update inventory SO.");
        }
    }

    public void RemoveItem()
    {
        if (items.Count > 0)
        {

            playerInventorySO.items.RemoveAt(currentItemIndex);
            playerInventorySO.currentItemIndex = 0;
            currentItemIndex = 0;
            ItemRefresh();
        }
    }

    public bool RemoveItemAtIndex(int itemIndex)
    {
        if (items.Count > 0 && itemIndex >= 0 && itemIndex < items.Count)
        {
            Debug.Log("itemIndex: " + itemIndex);
            playerInventorySO.items.RemoveAt(itemIndex);
            currentItemIndex = 0;
            ItemRefresh();
            return true;
        }
        Debug.LogError("Wrong index passed to RemoveItemAtIndex");
        return false;
    }

    private void ItemRefresh()
    {
        itemNameText.text = items[currentItemIndex].Name;
        ItemPreview();
    }

    public int GetItemIndex(string itemiD)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Id == itemiD)
            {
                ItemContractSO FoundItem = items[i];
                return i;
            }
        }
        Debug.Log("Item not found");
        return -1;
    }

    public string GetEquippedItemKey()
    {
        if (items.Count > 0 && currentItemIndex >= 0 && currentItemIndex < items.Count)
        {
            return items[currentItemIndex].Id;
        }

        return null;
    }

    public bool UseItemByItemKey(int itemKey)
    {
        int itemIndex = currentItemIndex;
        if (itemIndex >= 0)
        {
            if (RemoveItemAtIndex(itemIndex))
            {
                return true;
            }
        }
        return false;
    }

    public bool UseCurrentItem()
    {
        //TODO:
        //When the item is used, check if the interactable is not null
        //Check if the item key is the same as the itemKey of the interactable
        //if both checks pass, remove the item from the inventory and call the interactable's Interact method.
        return true;
    }

    private void RemoveTwoItems()
    {
        // Remove both items safely by removing the higher index first so the second index stays valid
                int firstIndex = Mathf.Max(itemFromIndex, itemToIndex);
                int secondIndex = Mathf.Min(itemFromIndex, itemToIndex);

                if (playerInventorySO != null)
                {
                    if (firstIndex >= 0 && firstIndex < playerInventorySO.items.Count)
                        playerInventorySO.items.RemoveAt(firstIndex);

                    if (secondIndex >= 0 && secondIndex < playerInventorySO.items.Count)
                        playerInventorySO.items.RemoveAt(secondIndex);
                    
                    currentItemIndex = playerInventorySO.items.Count-1;
                    playerInventorySO.currentItemIndex = currentItemIndex;
                    ItemRefresh();
                }
    }

    public void Combine()
    {
        if (itemFrom == null)
        {
            itemFrom = items[currentItemIndex];
            itemFromIndex = currentItemIndex;
            Debug.Log("ItemFrom: " + itemFrom.name + " at itemFromIndex: " + itemFromIndex);
        }
        else if(itemTo == null && currentItemIndex != itemFromIndex)
        {
            itemTo = items[currentItemIndex];
            itemToIndex = currentItemIndex;
            List<ItemContractSO> itemIngredients = new List<ItemContractSO>
            {
                itemFrom,
                itemTo
            };
            ItemContractSO result = recipeBook.FindRecipe(itemIngredients);
            if (result != null)
            {
                Debug.Log("itemFromIndex: " + itemFromIndex + ". The count  is " + items.Count);
                AddItem(result);
                RemoveTwoItems();
            }
            itemFrom = null;
            itemTo = null;
            itemFromIndex = -1;
            itemToIndex = -1;
        }
        
    }

}
