using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Tool,
    Material,
    Consumable,
    Clothing,
    Document
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Generic Information")]
    public int ID;
    public string itemName;
    public Sprite icon;
    public ItemCategory category;

    public bool isStackable;
    public int maxStackSize = 1;

    [Header("Durability")]
    public bool isBreakable;
    public float maxDurability = 0; // ignored if not breakable

    [Header("Cooking & Fuel")]
    public bool isFuel;
    public float fuelValue; // Seconds of fire provided

    public bool isCookable;
    public float cookTimeRequired;
    public ItemSO cookedResultSO; // What does this turn into?
}
