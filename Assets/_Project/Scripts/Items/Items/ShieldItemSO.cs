using UnityEngine;

public enum ShieldType
{
    Buckler,
    Medium,
    Tower
}

[CreateAssetMenu(fileName = "NewShieldItem", menuName = "Inventory/Shield Item")]
public class ShieldItemSO : ArmorItemSO
{
    [Header("Shield Stats")]
    public ShieldType shieldType;

    public float blockStrength = 50f;     // how much damage it can absorb
    public float staminaCost = 10f;       // stamina lost when blocking
    public float knockbackResist = 25f;   // reduces stagger when blocking
}
