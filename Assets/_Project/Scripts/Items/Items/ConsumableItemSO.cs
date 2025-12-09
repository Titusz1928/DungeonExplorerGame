using UnityEngine;


public enum ConsumableType
{
    Food,
    Potion,  
    Medicine       
}

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Inventory/Consumable Item")]
public class ConsumableItemSO : ItemSO
{
    [Header("Type")]
    public ConsumableType consumableType;

    [Header("Properties")]
    public float healthAmount;
    public float staminaAmount;

}