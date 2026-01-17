using System.Collections.Generic;
using UnityEngine;

public enum GDocumentType
{
    Book,
    Page,
    Map,
    Painting,
    Scroll
}

[System.Serializable]
public class PageSideSO
{
    public int pageID;
    public Sprite background;
    [TextArea(3, 5)] public string defaultText;

    [Header("XP Reward")]
    public bool givesXP;
    public int xpAmount;
    public PlayerSkill skillType;

    [Header("Crafting Unlock")]
    [Tooltip("Set to 0 if this page unlocks no recipe.")]
    public int recipeIDToUnlock;
}

[System.Serializable]
public class PaperSO
{
    public PageSideSO frontSide;
    public PageSideSO backSide;
}

[CreateAssetMenu(fileName = "NewDocument", menuName = "Inventory/Document")]
public class DocumentSO : ItemSO
{
    [Header("Document Settings")]
    public GDocumentType docType;

    [Header("Content Hierarchy")]
    // If it's a Book/Page, we use papers.
    // If it's a Scroll/Painting, we might just use the first paper's front side.
    public List<PaperSO> papers = new List<PaperSO>();


    private void OnValidate()
    {
        isStackable = false;
        maxStackSize = 1;
        category = ItemCategory.Document;
    }
}