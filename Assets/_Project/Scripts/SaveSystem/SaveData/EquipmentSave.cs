using System.Collections.Generic;

[System.Serializable]
public class EquipmentSave
{
    public int mainHandItemId;
    public int shieldItemId;

    // Slot → Layer → ItemID
    public Dictionary<ArmorSlot, Dictionary<ArmorLayer, int>> armor
        = new Dictionary<ArmorSlot, Dictionary<ArmorLayer, int>>();
}
