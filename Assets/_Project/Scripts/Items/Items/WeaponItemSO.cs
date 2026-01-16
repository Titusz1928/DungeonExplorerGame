using UnityEngine;


public enum WeaponType
{
    Bow,
    Knife,
    ShortSword,
    LongSword,
    Axe,
    Blunt,
    Spear
}

public enum DamageType
{
    Pierce,
    Blunt,
    Slash
}


[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Inventory/Weapon Item")]
public class WeaponItemSO : ItemSO
{
    [Header("Type")]
    public WeaponType weaponType;

    [Header("Damage")]
    public DamageType damageType;
    public float damageAmount;

    [Header("Audio")]
    public AudioClip attackSound;
}