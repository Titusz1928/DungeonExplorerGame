using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveEntry> entries = new List<ItemSaveEntry>();
}

[Serializable]
public class ItemSaveEntry
{
    public int itemID;
    public int quantity;
    public double durability;
}
