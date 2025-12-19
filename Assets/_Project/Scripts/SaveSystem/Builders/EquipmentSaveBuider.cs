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
        foreach (var slotEntry in eq.equippedArmor)
        {
            Dictionary<ArmorLayer, int> layerSave = new();

            foreach (var layerEntry in slotEntry.Value)
            {
                if (layerEntry.Value?.itemSO != null)
                {
                    layerSave[layerEntry.Key] = layerEntry.Value.itemSO.ID;
                }
            }

            if (layerSave.Count > 0)
                save.armor[slotEntry.Key] = layerSave;
        }

        return save;
    }

    public static void Apply(EquipmentManager eq, EquipmentSave save)
    {
        if (eq == null || save == null) return;

        // 1. Restore Main Hand
        if (save.mainHandItemId != 0)
        {
            ItemSO item = ItemDatabase.instance.GetByID(save.mainHandItemId);
            if (item is WeaponItemSO weaponSO)
            {
                // Create the "Instance" box first
                ItemInstance weaponInstance = new ItemInstance(weaponSO);

                // Now pass the instance to the manager
                eq.EquipMainHand(weaponInstance);
            }
        }

        // 2. Restore Shield
        if (save.shieldItemId != 0)
        {
            ItemSO item = ItemDatabase.instance.GetByID(save.shieldItemId);
            // Assuming your Shield SO is named ShieldItemSO
            if (item is ShieldItemSO shieldSO)
            {
                ItemInstance shieldInstance = new ItemInstance(shieldSO);
                eq.EquipShield(shieldInstance);
            }
        }

        // 3. Restore Armor
        if (save.armor != null)
        {
            foreach (var slotKvp in save.armor)
            {
                foreach (var layerKvp in slotKvp.Value)
                {
                    ItemSO item = ItemDatabase.instance.GetByID(layerKvp.Value);
                    if (item is ArmorItemSO armorSO)
                    {
                        ItemInstance armorInstance = new ItemInstance(armorSO);
                        eq.EquipArmor(armorInstance);
                    }
                }
            }
        }
    }
}
