using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct IngredientSlot
{
    public ItemSO item;
    public int quantity;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public int recipeID; // Linked to KnowledgeManager discoveredPageIDs
    public ItemSO resultItem;
    public int resultQuantity = 1;

    [Header("Ingredients")]
    // Using a list of slots allows you to require multiple different items
    public List<IngredientSlot> ingredients = new List<IngredientSlot>();

}