using System.Collections.Generic;

[System.Serializable]
public class EquipmentSave
{
    public int mainHandItemId;
    public int shieldItemId;

   public List<int> equippedArmorIds = new List<int>();
}
