using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveEntry> items = new List<ItemSaveEntry>();
}

[Serializable]
public class ItemSaveEntry
{
    public int itemID;
    public int quantity;
    public int durability;
}
