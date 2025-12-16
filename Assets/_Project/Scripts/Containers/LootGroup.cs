using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootGroup
{
    public string groupName;

    public List<ItemSO> possibleItems;

    [Min(0)] public int minCount = 0;
    [Min(0)] public int maxCount = 1;

    public bool allowDuplicates = false;
}