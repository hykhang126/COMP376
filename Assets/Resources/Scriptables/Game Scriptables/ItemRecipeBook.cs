using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRecipeBook", menuName = "Scriptable Objects/ItemRecipeBook")]
public class ItemRecipeBook : ScriptableObject
{
    //List of recipes to make an object.
    public List<ItemRecipe> recipeBook = new List<ItemRecipe>();

    public ItemContractSO FindRecipe(List<ItemContractSO> items)
    {
        ItemContractSO result = null;
        foreach (ItemRecipe recipe in recipeBook)
        {
            result = recipe.GetResult(items);
            if (result != null)
            {
                return result;
            }
        }
        return result;
    }
}
