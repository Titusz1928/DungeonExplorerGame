using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InjuryDatabase", menuName = "UI/Injury Database")]
public class InjuryDatabase : ScriptableObject
{
    [Header("Injury Types")]
    public Sprite cutIcon;
    public Sprite stabIcon;
    public Sprite fractureIcon;
    public Sprite unknownIcon; // The "?" for low Battle IQ

    [Header("Body Parts")]
    public List<BodyPartSprite> bodyPartSprites;

    public Sprite GetBodyPartSprite(ArmorSlot slot)
    {
        var match = bodyPartSprites.Find(s => s.slot == slot);
        return match.sprite != null ? match.sprite : unknownIcon;
    }
}

[System.Serializable]
public struct BodyPartSprite
{
    public ArmorSlot slot;
    public Sprite sprite;
}