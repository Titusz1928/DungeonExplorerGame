using System.Collections.Generic;

[System.Serializable]
public class InjurySaveEntry
{
    public ArmorSlot bodyPart;
    public InjuryType type;
    public float severity;
    public float healingRate;
    public bool isBandaged;
    public float bandageLifetime;
    public bool bandageDirty;
}

[System.Serializable]
public class InjurySaveData
{
    public List<InjurySaveEntry> injuries = new List<InjurySaveEntry>();
}