using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Containers/Grouped Container")]
public class GroupedContainerSO : ContainerSO
{
    public List<LootGroup> lootGroups;

    public override List<ItemInstance> GenerateLoot()
    {
        var result = new List<ItemInstance>();

        foreach (var group in lootGroups)
        {
            if (group.possibleItems.Count == 0)
                continue;

            int count = Random.Range(group.minCount, group.maxCount + 1);
            var available = new List<ItemSO>(group.possibleItems);

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = Random.Range(0, available.Count);
                ItemSO item = available[index];

                // --- ADD THIS SAFETY CHECK ---
                if (item == null)
                {
                    Debug.LogWarning($"[LootGen] A null item was found in a LootGroup on {this.name}!");
                    available.RemoveAt(index); // Remove it so we don't try again
                    i--; // Adjust index to try the loop iteration again
                    continue;
                }
                // -----------------------------

                int qty = item.isStackable
                    ? Random.Range(1, item.maxStackSize + 1)
                    : 1;

                // CHANGED: Create a new ItemInstance instead of a tuple
                result.Add(new ItemInstance(item, qty));

                if (!group.allowDuplicates)
                    available.RemoveAt(index);
            }
        }

        return result;
    }
}
