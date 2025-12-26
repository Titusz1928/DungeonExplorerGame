using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct FixedLootEntry
{
    public ItemSO item;
    public int qty;
}

[CreateAssetMenu(menuName = "Items/Containers/Fixed Container")]
public class FixedContainerSO : ContainerSO
{
    // Use the struct instead of tuples for Inspector visibility
    public List<FixedLootEntry> fixedItems;

    public override List<ItemInstance> GenerateLoot()
    {
        var result = new List<ItemInstance>();

        foreach (var entry in fixedItems)
        {
            if (entry.item != null)
            {
                // Create a fresh instance for the container
                result.Add(new ItemInstance(entry.item, entry.qty));
            }
        }

        return result;
    }
}