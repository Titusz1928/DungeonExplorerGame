using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "World/Structure Variation")]
public class StructureVariationSO : ScriptableObject
{
    [System.Serializable]
    public class VariationRule
    {
        public string gameObjectName;
        [Range(0, 1)] public float disappearanceChance;
    }

    public List<VariationRule> rules = new List<VariationRule>();
}