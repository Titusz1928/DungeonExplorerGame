using System.Collections.Generic;
using UnityEngine;

public static class EquipmentSaveBuilder
{
    public static EquipmentSave Build()
    {
        EquipmentSave save = new EquipmentSave();

        EquipmentManager eq = EquipmentManager.Instance;
        if (eq == null)
        {
            Debug.LogError("EquipmentManager.Instance is null! Cannot save equipment.");
            return save;
        }

        // --------------------
        // MAIN HAND
        // --------------------
        if (eq.mainHandWeapon != null && eq.mainHandWeapon.itemSO != null)
            save.mainHandItemId = eq.mainHandWeapon.itemSO.ID;

        // --------------------
        // SHIELD
        // --------------------
        if (eq.offHandShield != null && eq.offHandShield.itemSO != null)
            save.shieldItemId = eq.offHandShield.itemSO.ID;

        // --------------------
        // ARMOR (ALL LAYERS)
        // --------------------
        // Use a HashSet to avoid duplicates if one item covers multiple slots
        HashSet<int> uniqueArmorIds = new HashSet<int>();

        foreach (var slotEntry in eq.equippedArmor)
        {
            foreach (var layerEntry in slotEntry.Value)
            {
                if (layerEntry.Value?.itemSO != null)
                {
                    uniqueArmorIds.Add(layerEntry.Value.itemSO.ID);
                }
            }
        }

        // Convert the HashSet back to the serializable List
        save.equippedArmorIds = new List<int>(uniqueArmorIds);

        return save;
    }


    public static void Apply(EquipmentManager eq, Inventory inv, EquipmentSave save)
    {
        if (eq == null || save == null || inv == null) return;

        // Helper function to find an unequipped item in inventory by ID
        ItemInstance FindInInventory(int id)
        {
            return inv.items.Find(i => i.itemSO.ID == id && !i.isEquipped);
        }

        // 1. Restore Main Hand
        if (save.mainHandItemId != 0)
        {
            ItemInstance weapon = FindInInventory(save.mainHandItemId);
            if (weapon != null) eq.EquipMainHand(weapon);
        }

        // 2. Restore Shield
        if (save.shieldItemId != 0)
        {
            ItemInstance shield = FindInInventory(save.shieldItemId);
            if (shield != null) eq.EquipShield(shield);
        }

        // 3. Restore Armor
        if (save.equippedArmorIds != null)
        {
            foreach (int itemId in save.equippedArmorIds)
            {
                ItemInstance armor = FindInInventory(itemId);
                if (armor != null)
                {
                    eq.EquipArmor(armor, false);
                }
            }
        }
    }
}
