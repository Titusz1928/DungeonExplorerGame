using UnityEngine;


public enum ConsumableType
{
    Food,
    Potion,  
    FirstAid      
}

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Inventory/Consumable Item")]
public class ConsumableItemSO : ItemSO
{
    [Header("Type")]
    public ConsumableType consumableType;

    [Header("Properties")]
    public float healthAmount;
    public float staminaAmount;

    [Header("Container Return")]
    [Tooltip("The item given back to the player after consumption (e.g., an empty bottle). Leave null for nothing.")]
    public ItemSO returnItem;

    [Header("Audio")]
    public AudioClip useSound;

}