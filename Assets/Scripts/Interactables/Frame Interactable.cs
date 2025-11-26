using UnityEngine;

public class FrameInteractable : Interactable
{
    [SerializeField] private Renderer imageRenderer;

    [SerializeField] private Material frameMaterialNormal;

    [SerializeField] private Material frameMaterialTrigered;

    [SerializeField] private bool isOneTime = false;
    [SerializeField] private bool isTriggered = false;

    Color originalColor;

    public void Start()
    {
        imageRenderer.material = frameMaterialNormal;
        originalColor = imageRenderer.material.color;
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
                imageRenderer.material.color = originalColor;
                Inventory.InstanceReference.RemoveItem();
                return;
            }
        }

        isTriggered = !isTriggered;
        if (isTriggered)
        {
            imageRenderer.material = frameMaterialTrigered;
        }
        else
        {
            imageRenderer.material = frameMaterialNormal;
        }

        imageRenderer.material.color = originalColor;
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
