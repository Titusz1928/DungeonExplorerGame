using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Tool,
    Material,
    Consumable,
    Clothing
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public int ID;
    public string itemName;
    public Sprite icon;
    public ItemCategory category;

    public bool isStackable;
    public int maxStackSize = 1;

    public bool isBreakable;
    public double maxDurability = 0; // ignored if not breakable
}
