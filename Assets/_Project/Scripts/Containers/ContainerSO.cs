using UnityEngine;
using System.Collections.Generic;

public abstract class ContainerSO : ScriptableObject
{
    public string containerName;
    public Sprite containerIcon;

    // This method defines HOW loot is generated
    public abstract List<(ItemSO item, int qty)> GenerateLoot();
}
