using System.Collections.Generic;
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

[System.Serializable]
public struct ArmorDefense
{
    public float blunt;
    public float pierce;
    public float slash;
}

[System.Serializable]
public class ArmorSlotDefense
{
    public ArmorSlot slot;          // e.g. Torso
    public ArmorDefense defense;    // blunt, pierce, slash
}

[CreateAssetMenu(fileName = "NewArmorItem", menuName = "Inventory/Armor Item")]
public class ArmorItemSO : ItemSO
{
    [Header("Armor Position")]
    public ArmorSlot slotsCovered;
    public ArmorLayer layer;

    [Header("Defense Per Body Part")]
    public List<ArmorSlotDefense> defenses = new();

    [Header("Stealth")]
    public float noise;
    public float conspicuousness;

}