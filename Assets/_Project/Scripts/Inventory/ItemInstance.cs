[System.Serializable]
public class ItemInstance
{
    public ItemSO itemSO;
    public double currentDurability;
    public int quantity;

    public ItemInstance(ItemSO so, int quantity = 1)
    {
        itemSO = so;
        this.quantity = quantity;
        currentDurability = so.isBreakable ? so.maxDurability : 0;
    }
}
