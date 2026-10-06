using System.Collections.Generic;

public interface IKnowledgeService
{
    void TryReadPage(PageSideSO page);
    List<int> GetReadPageIDs();
    void LoadReadPages(List<int> loadedIDs);
    void UnlockRecipe(int recipeID);
    void GrantStartingRecipes(List<int> recipeIDs);
    bool IsRecipeKnown(int recipeID);
    List<int> GetKnownRecipeIDs();
    void LoadKnowledge(List<int> pages, List<int> recipes);
}