using System.Collections.Generic;

public static class KnowledgeSaveBuilder
{
    public static KnowledgeSaveData Build()
    {
        KnowledgeSaveData data = new KnowledgeSaveData();

        if (KnowledgeManager.Instance != null)
        {
            // Convert the HashSet to a List for the save file
            data.readPageIDs = KnowledgeManager.Instance.GetReadPageIDs();
        }

        return data;
    }

    public static void Apply(KnowledgeSaveData data)
    {
        if (data == null || KnowledgeManager.Instance == null) return;

        // Pass the list back to the manager to rebuild the HashSet
        KnowledgeManager.Instance.LoadReadPages(data.readPageIDs);
    }
}