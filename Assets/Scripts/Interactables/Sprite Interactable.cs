using UnityEngine;

public class SpriteInteractable : Interactable
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite spriteNormal;

    [SerializeField] private Sprite spriteTriggered;

    [SerializeField] private bool isOneTime = false;
    [SerializeField] private bool isTriggered = false;

    Color originalColor;

    public void Start()
    {
        spriteRenderer.sprite = spriteNormal;
        originalColor = spriteRenderer.color;
    }

    // One time interaction
    public override void Interact()
    {
        if (isOneTime && isTriggered)
        {
            return;
        }

        if(Inventory.InstanceReference != null && Inventory.InstanceReference.playerInventorySO.items.Count > 0)
        {
            if(ChangeMaterialColor())
            {
                spriteRenderer.color = originalColor;
                Inventory.InstanceReference.RemoveItem();
                return;
            }
        }

        isTriggered = !isTriggered;
        if (isTriggered)
        {
            spriteRenderer.sprite = spriteTriggered;
        }
        else
        {
            spriteRenderer.sprite = spriteNormal;
        }

        spriteRenderer.color = originalColor;
    }

    private bool ChangeMaterialColor()
    {
        switch(Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Name)
        {
            case "Red Vial":
                originalColor = Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Material.color;
                return true;
            case "Blue Vial":
                originalColor = Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Material.color;
                return true;
            case "Green Vial":
                originalColor = Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Material.color;
                return true;
            case "Yellow Vial":
                originalColor = Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Material.color;
                return true;
            case "Magenta Vial":
                originalColor = Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Material.color;
                return true;
            case "Cyan Vial":
                originalColor = Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Material.color;
                return true;
            default:
                Debug.LogWarning("Item used on frame is not a paint item.");
                return false;
        }
    }
}
