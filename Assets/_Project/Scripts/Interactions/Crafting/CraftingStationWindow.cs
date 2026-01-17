using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class CraftingStationWindow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject rowPrefab;

    [Header("Containers")]
    public Transform rowParent;

    [Header("Design")]
    public Image stationPreviewImage;
    public TextMeshProUGUI stationName;

    private CraftingStation currentStation;
    private Inventory playerInventory;

    public void Initialize(CraftingStation station, Sprite stationSprite)
    {
        currentStation = station;

        // Find the player's inventory (assumes PersistentPlayer is tagged "Player")
        if (playerInventory == null)
        {
            playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        }

        if (stationPreviewImage != null && stationSprite != null)
        {
            stationPreviewImage.sprite = stationSprite;

            // Optional: Maintain aspect ratio if your UI image isn't square
            stationPreviewImage.preserveAspect = true;
        }

        stationName.text = station.interactText;

        RefreshUI();
    }

    public void RefreshUI()
    {
        // 1. Clear current list
        foreach (Transform child in rowParent) Destroy(child.gameObject);

        // 2. Filter and Spawn Recipes
        foreach (RecipeSO recipe in currentStation.availableRecipes)
        {
            // Only show if known by the player
            if (KnowledgeManager.Instance.IsRecipeKnown(recipe.recipeID))
            {
                SpawnRow(recipe);
            }
        }
    }

    private void SpawnRow(RecipeSO recipe)
    {
        GameObject rowGo = Instantiate(rowPrefab, rowParent);
        CraftingRowUI rowUI = rowGo.GetComponent<CraftingRowUI>();

        // Check if player has the items right now
        bool canCraft = playerInventory.HasIngredients(recipe.ingredients);

        // Pass the logic to the row
        rowUI.Setup(recipe, canCraft, () => ExecuteCraft(recipe));
    }

    private void ExecuteCraft(RecipeSO recipe)
    {
        // Double check ingredients before final execution
        if (playerInventory.HasIngredients(recipe.ingredients))
        {
            playerInventory.RemoveIngredients(recipe.ingredients);
            playerInventory.AddItem(recipe.resultItem, recipe.resultQuantity);

            Debug.Log($"Successfully crafted {recipe.resultItem.itemName}");

            // IMPORTANT: Refresh UI because inventory counts have changed
            RefreshUI();
        }
    }
}