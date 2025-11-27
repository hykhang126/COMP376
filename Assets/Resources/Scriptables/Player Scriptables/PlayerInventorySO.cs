using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

[CreateAssetMenu(fileName = "PlayerInventorySO", menuName = "Scriptable Objects/PlayerInventorySO")]
public class PlayerInventorySO : MyScriptables
{
    public int currentItemIndex = 0;

    [Header("Player Items")]
    public List<ItemContractSO> items = new();

    [Header("Persistent Items")]
    public List<ItemContractSO> persistentItems = new();
    [Description("Items that should persist across scenes and game sessions. Assign items here to ensure they are not lost.")]
    public List<ItemContractSO> persistentItemList = new();

    [NaughtyAttributes.Button("Clear & Re-add Items")]
    public void ClearItemsThenReAdd()
    {
        ClearItemsInstance();
        foreach (var pair in persistentItems)
        {
            items.Add(pair);
        }
    }

    [NaughtyAttributes.Button("Clear Items data")]
    public void ClearItemsInstance()
    {
        items.Clear();
        currentItemIndex = 0;
        Debug.Log("PlayerInventorySO items cleared.");
    }

    [NaughtyAttributes.Button("Clear Persistent Items")]
    public void ClearPersistentItems()
    {
        persistentItems.Clear();
    }

    [NaughtyAttributes.Button("TEST: Generate dummy Items & Persistent")]
    public void GenerateDummyItems()
    {
        ClearItemsInstance();
        ClearPersistentItems();

        for (int i = 0; i < 4; i++)
        {
            ItemContractSO newItem = new ItemContractSO();
            items.Add(newItem);

            if (i == 0) // Make even indexed items persistent
            {
                persistentItems.Add(newItem);
            }
        }
    }

    [NaughtyAttributes.Button("DANGER: Clear All data")]
    override public void ClearAllData()
    {
        
        base.ClearAllData();
        currentItemIndex = 0;
        items.Clear();
        persistentItems.Clear();
        persistentItemList.Clear();
    }
}