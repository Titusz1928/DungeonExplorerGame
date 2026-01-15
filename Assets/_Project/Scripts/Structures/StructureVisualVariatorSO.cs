using System.Collections.Generic;
using UnityEngine;

public class StructureVisualVariator : MonoBehaviour
{
    public StructureVariationSO variationRules;
    public Transform objectsFolder; // Assign the "Objects" child folder here

    // This rolls the dice for a brand new house
    public List<int> GenerateVariation()
    {
        List<int> hiddenIndices = new List<int>();
        if (variationRules == null || objectsFolder == null) return hiddenIndices;

        for (int i = 0; i < objectsFolder.childCount; i++)
        {
            GameObject child = objectsFolder.GetChild(i).gameObject;

            // Check if there's a rule for this object name
            var rule = variationRules.rules.Find(r => r.gameObjectName == child.name);

            if (rule != null)
            {
                if (Random.value < rule.disappearanceChance)
                {
                    hiddenIndices.Add(i);
                    child.SetActive(false);
                }
            }
            // If no rule is found, it stays active (default behavior)
        }
        return hiddenIndices;
    }

    // This applies a variation loaded from a save file
    public void ApplyVariation(List<int> hiddenIndices)
    {
        if (objectsFolder == null || hiddenIndices == null) return;

        foreach (int index in hiddenIndices)
        {
            if (index < objectsFolder.childCount)
            {
                objectsFolder.GetChild(index).gameObject.SetActive(false);
            }
        }
    }
}