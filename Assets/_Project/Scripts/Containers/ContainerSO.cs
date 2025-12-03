using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/ContainerSO")]
public class ContainerSO : ScriptableObject
{
    public string containerName;

    // UI icon for the tab
    public Sprite containerIcon;

    // Which items can spawn?
    public List<ItemSO> allowedItems;

    public int minItems = 1;
    public int maxItems = 3;
}
