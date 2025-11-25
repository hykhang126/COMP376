using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEditor.ShaderKeywordFilter;

public class Inventory : MonoBehaviour
{
    // Global Variables
    public List<ItemContractSO> items = new List<ItemContractSO>();
    public GameObject inventoryUI; // Reference to the inventory UI GameObject
    public GameObject itemPreviewPlaceholder;
    public ItemRecipeBook recipeBook;

    public GameObject itemFromPreviewIndicator;
    public Camera inventoryCamera { get; private set; }

    public static Inventory InstanceReference { get; private set; }

    // Serialized
    [SerializeField] private PlayerInventorySO playerInventorySO;
    [SerializeField] private AudioClip pickUpAudioClip;
    [SerializeField] private int itemFromIndex = -1;
    [SerializeField] private int itemToIndex = -1;

    // Private
    private PlayerInputHandler playerInputHandler;
    private Flashlight flashlight;
    private int currentItemIndex = 0;
    private bool isInventoryOpen = false;
    private TextMeshProUGUI itemNameText;
    private ItemContractSO itemFrom;
    private ItemContractSO itemTo;
    private string previousPlayerState;
    private string playerMapName;
    private string inventoryMapName;

    public ItemContractSO flashlightContractSO;
    public ItemContractSO sandwichContractSO;

    public static UnityEvent rechargeEvent = new UnityEvent();
    public static UnityEvent sandwichEvent = new UnityEvent();
    public void Awake()
    {
        // Singleton pattern to ensure only one instance of Inventory exists
        if (InstanceReference == null)
        {
            InstanceReference = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup inventory camera
        inventoryCamera = GetComponentInChildren<Camera>();
        if (inventoryCamera == null)
        {
            Debug.LogError("Inventory Camera not found as a child of Inventory GameObject.");
        }
    }

    public void Start()
    {
        if (Player.InstanceReference != null)
        {
            playerInputHandler = Player.InstanceReference.playerInputHandler;
            if (playerInputHandler == null)
            {
                Debug.LogError("PlayerInputHandler is null on Player Instance Reference.");
            }
            else
            {
                // Get flashlight reference from PlayerInputHandler (may be null if not yet available)
                flashlight = playerInputHandler.flashlight;

            }
        }
        else
        {
            Debug.LogError("Player Instance Reference is null. Cannot access PlayerInputHandler.");
        }
        playerMapName = playerInputHandler.playerActionMap;
        inventoryMapName = playerInputHandler.inventoryActionMap;

        playerInputHandler.AddMapActionNoParamSubscriber(playerMapName, "InventoryToggle", ToggleInventory);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "InventoryToggle", ToggleInventory);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "Next", Next);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "Previous", Previous);
        playerInputHandler.AddMapActionNoParamSubscriber(inventoryMapName, "Combine", Combine);
        playerInputHandler.AddMapActionSubscriber(inventoryMapName, "CycleItems", CycleItems);

        Transform InventoryPanelTransform = inventoryUI.transform.Find("Panel");
        GameObject panel = InventoryPanelTransform != null ? InventoryPanelTransform.gameObject : null;
        Transform itemNameTransform = panel != null ? panel.transform.Find("ItemName") : null;
        itemNameText = itemNameTransform != null ? itemNameTransform.GetComponent<TextMeshProUGUI>() : null;
        inventoryUI.SetActive(false);
        itemFromPreviewIndicator = panel.transform.Find("ItemFromIndicator").gameObject;
        itemFromPreviewIndicator.SetActive(false);

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
        if (itemPreviewPlaceholder == null)
        {
            Debug.LogError("Did not find Item Preview Placeholder");
        }

    }
    
    void OnDisable()
    {
        // Unsubscribe from input events to prevent memory leaks
        playerInputHandler.RemoveMapActionNoParamSubscriber(playerMapName, "InventoryToggle", ToggleInventory);
        playerInputHandler.RemoveMapActionNoParamSubscriber(inventoryMapName, "InventoryToggle", ToggleInventory);
        playerInputHandler.RemoveMapActionNoParamSubscriber(inventoryMapName, "Next", Next);
        playerInputHandler.RemoveMapActionNoParamSubscriber(inventoryMapName, "Previous", Previous);
        playerInputHandler.RemoveMapActionNoParamSubscriber(inventoryMapName, "Combine", Combine);
        playerInputHandler.RemoveMapActionSubscriber(inventoryMapName, "CycleItems", CycleItems);
    }

    public void ToggleInventory()
    {
        if (this == null || !gameObject.scene.IsValid()) return; // Input safety check
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

        // Update flashlight reference if needed (in case it wasn't available at start)
        if (flashlight == null && playerInputHandler != null)
        {
            flashlight = playerInputHandler.flashlight;
        }

        // Turn off flashlight when opening inventory (only if flashlight exists and is activated)
        if (flashlight != null && flashlight.IsActivated)
        {
            flashlight.ToggleFlashlight();
        }

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
        Debug.Log("The item's material is: "+ items[currentItemIndex].Material);
        Material newMaterial = new Material(items[currentItemIndex].Material);
        itemPreviewPlaceholder.GetComponent<MeshRenderer>().material = newMaterial;

        if(currentItemIndex == itemFromIndex)
        {
            itemFromPreviewIndicator.SetActive(true);
        }
        else
        {
            itemFromPreviewIndicator.SetActive(false);
        }
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
        if (this == null || !gameObject.scene.IsValid()) return; // Input safety check
        
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
        if (this == null || !gameObject.scene.IsValid()) return; // Input safety check

        if (items.Count == 0 || !isInventoryOpen) return; // No items to cycle through
        currentItemIndex = (currentItemIndex + 1) % items.Count;
        playerInventorySO.currentItemIndex = currentItemIndex; // Update the current item index in the SO
        itemNameText.text = items[currentItemIndex].Name; // Update the item name text

        ItemPreview();
    }

    public void Previous()
    {
        if (this == null || !gameObject.scene.IsValid()) return; // Input safety check

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
        if(items.Count == 0)
        {
            itemNameText.text = "";
            itemPreviewPlaceholder.SetActive(false);
            return;
        }
        else
        {
            itemNameText.text = items[currentItemIndex].Name;
            ItemPreview();
        }
        
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
        if (this == null || !gameObject.scene.IsValid()) return; // Input safety check

        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("[Inventory] Combine aborted: inventory is empty or items list is null.");
            ResetCombineSelection();
            return;
        }
        if (currentItemIndex < 0 || currentItemIndex >= items.Count)
        {
            Debug.LogWarning($"[Inventory] Combine aborted: currentItemIndex ({currentItemIndex}) out of range.");
            ResetCombineSelection();
            return;
        }

        // First selection
        if (itemFrom == null)
        {
            itemFrom = items[currentItemIndex];
            itemFromIndex = currentItemIndex;

            if (itemFrom == null)
            {
                Debug.LogWarning("[Inventory] Combine aborted: selected item (itemFrom) is null in list.");
                ResetCombineSelection();
                return;
            }

            ItemPreview();

            return;
        }
        // Second selection (must be a different index)
        else if (itemTo == null && currentItemIndex != itemFromIndex)
        {
            itemTo = items[currentItemIndex];
            itemToIndex = currentItemIndex;

            if (itemTo == null)
            {
                Debug.LogWarning("[Inventory] Combine aborted: selected item (itemTo) is null in list.");
                ResetCombineSelection();
                return;
            }

            if (recipeBook == null)
            {
                Debug.LogError("[Inventory] Combine aborted: recipeBook is not assigned in the Inspector!");
                ResetCombineSelection();
                return;
            }

            // Build ingredient list and look up recipe
            List<ItemContractSO> itemIngredients = new List<ItemContractSO> { itemFrom, itemTo };
            ItemContractSO result = recipeBook.FindRecipe(itemIngredients);

            if (result == null)
            {
                Debug.Log($"[Inventory] No recipe found for {itemFrom.name} + {itemTo.name}");
                ResetCombineSelection();
                return;
            }

            // Safety checks for special-case contracts
            if (flashlightContractSO != null && result.Id == flashlightContractSO.Id)
            {
                rechargeEvent?.Invoke();

                if (playerInventorySO == null)
                {
                    Debug.LogError("[Inventory] Cannot remove items: playerInventorySO is null.");
                }
                else
                {
                    if (itemFrom != null && itemFrom.Id != flashlightContractSO.Id)
                    {
                        if (IsIndexValid(itemFromIndex)) playerInventorySO.items.RemoveAt(itemFromIndex);
                    }
                    else
                    {
                        if (IsIndexValid(itemToIndex)) playerInventorySO.items.RemoveAt(itemToIndex);
                    }
                }
            }
            else if (sandwichContractSO != null && result.Id == sandwichContractSO.Id)
            {
                sandwichEvent?.Invoke();
                AddItem(result);
                RemoveTwoItems();
            }
            else
            {
                AddItem(result);
                RemoveTwoItems();
            }

            // Reset selection at end
            ResetCombineSelection();
        }
        else
        {
            // Either same index or both already selected, safe reset
            ResetCombineSelection();
        }

        ItemPreview();
        
    }

    //Helper to reset selection and indices
    private void ResetCombineSelection()
    {
        itemFrom = null;
        itemTo = null;
        itemFromIndex = -1;
        itemToIndex = -1;
    }

    //Helper to validate index against current playerInventorySO.items if available, else items
    private bool IsIndexValid(int idx)
    {
        var listToCheck = (playerInventorySO != null) ? playerInventorySO.items : items;
        return listToCheck != null && idx >= 0 && idx < listToCheck.Count;
    }

}
