using System.Collections.Generic;
using UnityEngine;

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance;

    // This set stores all unique pageIDs the player has read.
    // In a real project, you should include this in your Save/Load system.
    private HashSet<int> discoveredPageIDs = new HashSet<int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
            PlayerSkillManager.Instance.AddXP(page.skillType, page.xpAmount);
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
}