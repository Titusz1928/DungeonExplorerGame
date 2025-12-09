using UnityEngine;

[System.Flags]
public enum ArmorSlot
{
    None = 0,
    Head = 1 << 0,
    Face = 1 << 1,
    Neck = 1 << 2,
    Torso = 1 << 3,
    Shoulders = 1 << 4,
    Arms = 1 << 5,
    Hands = 1 << 6,
    Legs = 1 << 7,
    Feet = 1 << 8
}

public enum ArmorLayer
{
    Under,      // padding, leather pants, gambeson
    Chainmail,  // hauberk, chain coif
    Plate,      // plate armor
    Over        // cloak, tabard
}

[CreateAssetMenu(fileName = "NewArmorItem", menuName = "Inventory/Armor Item")]
public class ArmorItemSO : ItemSO
{
    [Header("Armor Position")]
    public ArmorSlot slotsCovered;
    public ArmorLayer layer;

    [Header("Defense")]
    public float bluntRes;
    public float pierceRes;
    public float slashRes;

    [Header("Stealth")]
    public float noise;
    public float conspicuousness;

    [Header("Wear & Tear")]
    public int holes;
}