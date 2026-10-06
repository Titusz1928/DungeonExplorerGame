using System.Collections.Generic;

public interface IEquipmentService
{
    Dictionary<ArmorSlot, Dictionary<ArmorLayer, ItemInstance>> equippedArmor { get; }
    ItemInstance mainHandWeapon { get; }
    ItemInstance offHandShield { get; }

    void Equip(ItemInstance item);
    void Unequip(ItemInstance item);
    void EquipArmor(ItemInstance item, bool playSound = true);
    void UnequipArmor(ItemInstance item);
    void EquipMainHand(ItemInstance weapon);
    void UnequipMainHand(ItemInstance weapon);
    void EquipShield(ItemInstance shield);
    void UnequipShield(ItemInstance shield);

    bool IsArmorEquipped(ArmorSlot slot, ArmorLayer layer);
    ArmorDefense GetTotalDefenseForSlot(ArmorSlot slot);
    float GetTotalSneak();
    float GetTotalConspicuousness();
}