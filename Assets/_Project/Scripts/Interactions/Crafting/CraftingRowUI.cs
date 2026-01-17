using UnityEngine;

public class CraftingRowUI : MonoBehaviour
{
    [Header("Containers")]
    public Transform ingredientParent;
    public Transform resultParent;

    [Header("Prefabs")]
    public GameObject ingredientCellPrefab;
    public GameObject resultCellPrefab;

    [Header("Visuals")]
    public CanvasGroup canvasGroup;

    public void Setup(RecipeSO recipe, bool canCraft, System.Action onCraftClicked)
    {
        // 1. Clear old UI
        foreach (Transform t in ingredientParent) Destroy(t.gameObject);
        foreach (Transform t in resultParent) Destroy(t.gameObject);

        // 2. Setup Ingredients
        foreach (var ing in recipe.ingredients)
        {
            GameObject go = Instantiate(ingredientCellPrefab, ingredientParent);
            go.GetComponent<IngredientCell>().Setup(ing.item, ing.quantity);
        }

        // 3. Setup Result (The Button)
        GameObject resGo = Instantiate(resultCellPrefab, resultParent);
        ResultCell resultCell = resGo.GetComponent<ResultCell>();
        resultCell.Setup(recipe.resultItem, recipe.resultQuantity, canCraft, onCraftClicked);

        // 4. Row Visual State
        // If they can't craft, we dim the whole row to 0.5 alpha
        canvasGroup.alpha = canCraft ? 1.0f : 0.5f;
    }
}