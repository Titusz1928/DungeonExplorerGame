using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance;

    // This set stores all unique pageIDs the player has read.
    // In a real project, you should include this in your Save/Load system.
    private HashSet<int> discoveredPageIDs = new HashSet<int>();
    private HashSet<int> knownRecipeIDs = new HashSet<int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //BOOKS
    public void TryReadPage(PageSideSO page)
    {
        // 1. Safety check: If the page is null or ID is 0 (unassigned), do nothing.
        if (page == null || page.pageID == 0) return;

        // 2. Check if we've already read this specific page ID.
        if (discoveredPageIDs.Contains(page.pageID)) return;

        // 3. Mark as read.
        discoveredPageIDs.Add(page.pageID);
        Debug.Log($"Discovered new page ID: {page.pageID}");

        // 4. Grant XP if applicable.
        if (page.givesXP && page.xpAmount > 0)
        {
            PlayerSkillManager.Instance.AddXP(page.skillType, page.xpAmount, true);
        }

        if (page.recipeIDToUnlock > 0)
        {
            UnlockRecipe(page.recipeIDToUnlock);
        }
    }

    public List<int> GetReadPageIDs()
    {
        return new List<int>(discoveredPageIDs);
    }

    public void LoadReadPages(List<int> loadedIDs)
    {
        discoveredPageIDs = new HashSet<int>(loadedIDs);
        Debug.Log($"KnowledgeManager: Restored {discoveredPageIDs.Count} read pages.");
    }

    //RECIPES
    //to handle starting recipes
    public void UnlockRecipe(int recipeID)
    {
        if (recipeID == 0) return;

        if (!knownRecipeIDs.Contains(recipeID))
        {
            knownRecipeIDs.Add(recipeID);
            Debug.Log($"New Recipe Unlocked: {recipeID}");
            // You could trigger a UI notification here: "New Recipe Learned!"


           MessageManager.Instance.ShowMessageDirectly($"You unlocked a new recipe!");
        
        }
    }

    public void GrantStartingRecipes(List<int> recipeIDs)
    {
        foreach (int id in recipeIDs) UnlockRecipe(id);
    }

    public bool IsRecipeKnown(int recipeID)
    {
        if (recipeID == 0) return true; // Basic recipes
        return knownRecipeIDs.Contains(recipeID);
    }

    // --- SAVE / LOAD HELPERS ---
    public List<int> GetKnownRecipeIDs() => new List<int>(knownRecipeIDs);

    public void LoadKnowledge(List<int> pages, List<int> recipes)
    {
        discoveredPageIDs = new HashSet<int>(pages);
        knownRecipeIDs = new HashSet<int>(recipes);
        Debug.Log($"Knowledge Restored: {discoveredPageIDs.Count} pages, {knownRecipeIDs.Count} recipes.");
    }
}