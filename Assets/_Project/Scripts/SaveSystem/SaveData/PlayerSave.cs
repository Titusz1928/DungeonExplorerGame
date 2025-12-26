using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave
{
    public Vector2 position;

    public float health;
    public float stamina;

    public InventorySaveData inventory;
    public EquipmentSave equipment;
    public InjurySaveData injuries;
    public List<SkillSaveEntry> skills;
}
