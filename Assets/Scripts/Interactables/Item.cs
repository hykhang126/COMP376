using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Item
{
    public ItemContractSO itemContractSO;

    public Item(ItemContractSO itemContractSO)
    {
        this.itemContractSO = itemContractSO;
    }
}
