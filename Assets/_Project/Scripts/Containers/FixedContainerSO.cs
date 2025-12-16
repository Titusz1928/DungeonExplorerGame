using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Containers/Fixed Container")]
public class FixedContainerSO : ContainerSO
{
    public List<(ItemSO item, int qty)> fixedItems;

    public override List<(ItemSO item, int qty)> GenerateLoot()
    {
        return new List<(ItemSO, int)>(fixedItems);
    }
}
