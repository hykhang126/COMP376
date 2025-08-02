using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerInventorySO", menuName = "Scriptable Objects/PlayerInventorySO")]
public class PlayerInventorySO : MyScriptables
{
    public List<Item> items = new List<Item>();

    public int currentItemIndex = 0;

    public List<GameObject> itemList = new List<GameObject>();

    [NaughtyAttributes.Button("Clear All data")]
    override public void ClearAllData()
    {
        base.ClearAllData();
        items.Clear();
        currentItemIndex = 0;
        itemList.Clear();
        Debug.Log("PlayerInventorySO data cleared.");
    }
}
