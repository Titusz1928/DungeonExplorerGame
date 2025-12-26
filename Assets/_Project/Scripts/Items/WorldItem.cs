using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemSO itemSO;
    public int quantity = 1;
    public double currentDurability;

    public void Initialize(ItemInstance instance)
    {
        itemSO = instance.itemSO;
        quantity = instance.quantity;
        currentDurability = instance.currentDurability;
    }
}
