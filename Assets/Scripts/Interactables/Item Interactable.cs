using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
//Needs MeshRenderer component
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ItemInteractable : Interactable
{

    [SerializeField] private ItemContractSO itemContractSO;

    MeshFilter _meshFilter;

    MeshRenderer _meshRenderer;

    MeshCollider _meshCollider;

    [SerializeField] private PlayerInventorySO playerInventorySO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (itemContractSO == null)
        {
            Debug.LogError("ItemContractSO is not assigned in the inspector.");
            return;
        }
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        //Set the mesh of the item to the mesh from the ItemContractSO
        if (itemContractSO.MeshRef != null)
        {
            _meshFilter.mesh = itemContractSO.MeshRef;
        }
        else
        {
            Debug.LogWarning("MeshRef is not assigned in ItemContractSO: " + itemContractSO.Name);
        }

        if (itemContractSO.Material != null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.material = itemContractSO.Material;
        }
        else
        {
            Debug.LogWarning("Material is not assigned in ItemContractSO: " + itemContractSO.Name);
        }
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }

    public override void Interact(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Player is null, cannot interact with item.");
            return;
        }
        else if (!player.inventory)
        {
            Debug.LogError("Player's inventory is null, cannot add item.");
            return;
        }
        else
        {
            player.inventory.AddItem(itemContractSO);
            // player.inventory.AddItem(item.itemName, item.itemKey, item);
            Destroy(gameObject);
        }
    }

    // Get the item prefab from the playerInventorySO
    public GameObject GetItemPrefab(int itemKey)
    {
        if (playerInventorySO != null)
        {
            foreach (var pair in playerInventorySO.itemList)
            {
                if (pair.Key == itemKey)
                {
                    return pair.Value;
                }
            }
        }
        Debug.LogWarning("Item prefab not found for key: " + itemKey);
        return null;
    }

    private void OnValidate()
    {
        if(itemContractSO != null)
        {
            if (itemContractSO.MeshRef != null)
            {
                GetComponent<MeshFilter>().mesh = itemContractSO.MeshRef;
            }
            else
            {
                Debug.LogWarning("MeshRef is not assigned in ItemContractSO: " + itemContractSO.Name);
            }

            if (itemContractSO.Material != null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                _meshRenderer.material = itemContractSO.Material;
            }
            else
            {
                Debug.LogWarning("Material is not assigned in ItemContractSO: " + itemContractSO.Name);
            }
            GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
