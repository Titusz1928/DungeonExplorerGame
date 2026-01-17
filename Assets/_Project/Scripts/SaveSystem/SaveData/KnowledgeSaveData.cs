using System.Collections.Generic;

[System.Serializable]
public class KnowledgeSaveData
{
    // A simple list of IDs is easiest to save and load
    public System.Collections.Generic.List<int> readPageIDs = new System.Collections.Generic.List<int>();
    public List<int> knownRecipeIDs = new List<int>();
}