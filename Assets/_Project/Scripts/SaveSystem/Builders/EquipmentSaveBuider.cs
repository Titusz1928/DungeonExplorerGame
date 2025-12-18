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
}
