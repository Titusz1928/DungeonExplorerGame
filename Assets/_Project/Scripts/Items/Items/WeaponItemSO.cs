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

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Inventory/Weapon Item")]
public class WeaponItemSO : ItemSO
{
    [Header("Type")]
    public WeaponType weaponType;

    [Header("Properties")]
    public float pierceDamage;
    public float bluntDamage;
    public float slashDamage;


}