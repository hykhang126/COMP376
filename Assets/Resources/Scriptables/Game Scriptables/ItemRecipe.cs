using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRecipe", menuName = "Scriptable Objects/ItemRecipe")]
public class ItemRecipe : ScriptableObject
{
    public List<ItemContractSO> itemIngredients = new List<ItemContractSO>();

    [SerializeField] private ItemContractSO result;

    public ItemContractSO GetResult(List<ItemContractSO>items)
    {
        bool firstFound = false;
        bool secondFound = false;
        foreach (ItemContractSO item in items)
        {
            for (int i = 0; i < itemIngredients.Count; i++)
            {
                if (i == 0 && firstFound) continue;
                if (i == 1 && secondFound) continue;
                if (item.Id.Equals(itemIngredients[i].Id))
                {
                    if (i == 0) firstFound = true;
                    if (i == 1) secondFound = true;
                    break;
                }
            }
        }

        if (firstFound && secondFound) return result;
        return null;

    }


}
