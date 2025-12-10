[System.Serializable]
public class ItemInstance
{
    public ItemSO itemSO;
    public double currentDurability;
    public int quantity;
    public bool isEquipped;

    // Armor-only dynamic stat
    public int holes = 0;

    public ItemInstance(ItemSO so, int quantity = 1)
    {
        itemSO = so;
        this.quantity = quantity;
        currentDurability = so.isBreakable ? so.maxDurability : 0;
    }
}
