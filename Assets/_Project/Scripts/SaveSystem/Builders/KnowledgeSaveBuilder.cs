using System.Collections.Generic;
using TitusGames.Framework;

public static class KnowledgeSaveBuilder
{
    public static KnowledgeSaveData Build()
    {
        KnowledgeSaveData data = new KnowledgeSaveData();

        ServiceLocator.Current.TryGet<IKnowledgeService>(out var knowledgeService);
        if (knowledgeService != null)
        {
            // Convert the HashSet to a List for the save file
            data.readPageIDs = knowledgeService.GetReadPageIDs();
            data.knownRecipeIDs = knowledgeService.GetKnownRecipeIDs();
        }

        return data;
    }

    public static void Apply(KnowledgeSaveData data)
    {
        ServiceLocator.Current.TryGet<IKnowledgeService>(out var knowledgeService);
        if (data == null || knowledgeService == null) return;

        // Pass the list back to the manager to rebuild the HashSet
        knowledgeService.LoadKnowledge(data.readPageIDs, data.knownRecipeIDs);
    }
}