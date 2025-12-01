using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemInstance> items = new List<ItemInstance>();
    public int maxSlots = 20;

    public bool AddItem(ItemSO itemSO, int amount = 1)
    {
        // stacking logic
        if (itemSO.isStackable)
        {
            foreach (var item in items)
            {
                if (item.itemSO == itemSO && item.quantity < itemSO.maxStackSize)
                {
                    int spaceLeft = itemSO.maxStackSize - item.quantity;
                    int toAdd = Mathf.Min(spaceLeft, amount);

                    item.quantity += toAdd;
                    amount -= toAdd;

                    if (amount <= 0)
                        return true;
                }
            }
        }

        // add new slots
        while (amount > 0 && items.Count < maxSlots)
        {
            int addAmount = itemSO.isStackable ? Mathf.Min(itemSO.maxStackSize, amount) : 1;

            items.Add(new ItemInstance(itemSO, addAmount));
            amount -= addAmount;
        }

        return amount <= 0;
    }

    public bool RemoveItem(ItemSO itemSO, int amount = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var inst = items[i];

            if (inst.itemSO == itemSO)
            {
                if (inst.quantity > amount)
                {
                    // Just decrease quantity
                    inst.quantity -= amount;
                    return true;
                }
                else
                {
                    // Remove entire stack
                    items.RemoveAt(i);
                    return true;
                }
            }
        }

        return false; // item not found
    }



    public void Save()
    {
        InventorySaveData data = new InventorySaveData();

        foreach (var item in items)
        {
            data.items.Add(new ItemSaveEntry
            {
                itemID = item.itemSO.ID,
                quantity = item.quantity,
                durability = item.currentDurability
            });
        }

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("inventory", json);
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString("inventory", "");
        if (json == "") return;

        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

        items.Clear();

        foreach (var entry in data.items)
        {
            ItemSO so = ItemDatabase.instance.GetByID(entry.itemID);

            ItemInstance inst = new ItemInstance(so, entry.quantity);
            inst.currentDurability = entry.durability;

            items.Add(inst);
        }
    }
}
