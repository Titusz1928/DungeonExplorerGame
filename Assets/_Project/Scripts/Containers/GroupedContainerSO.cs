using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Containers/Grouped Container")]
public class GroupedContainerSO : ContainerSO
{
    public List<LootGroup> lootGroups;

    public override List<(ItemSO item, int qty)> GenerateLoot()
    {
        var result = new List<(ItemSO, int)>();

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

                int qty = item.isStackable
                    ? Random.Range(1, item.maxStackSize + 1)
                    : 1;

                result.Add((item, qty));

                if (!group.allowDuplicates)
                    available.RemoveAt(index);
            }
        }

        return result;
    }
}
