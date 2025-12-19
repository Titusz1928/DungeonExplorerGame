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
            Debug.Log(items.Count+" < "+maxSlots);
            int addAmount = itemSO.isStackable ? Mathf.Min(itemSO.maxStackSize, amount) : 1;

            items.Add(new ItemInstance(itemSO, addAmount));
            amount -= addAmount;
        }

        return amount <= 0;
    }

    public bool RemoveItem(ItemInstance instance, int amount = 1)
    {
        if (!items.Contains(instance))
            return false;

        if (instance.quantity > amount)
        {
            instance.quantity -= amount;
            return true;
        }
        else
        {
            items.Remove(instance);
            return true;
        }
    }



    public void Save()
    {
        InventorySaveData data = new InventorySaveData();

        foreach (var item in items)
        {
            data.entries.Add(new ItemSaveEntry
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

        foreach (var entry in data.entries)
        {
            ItemSO so = ItemDatabase.instance.GetByID(entry.itemID);

            ItemInstance inst = new ItemInstance(so, entry.quantity);
            inst.currentDurability = entry.durability;

            items.Add(inst);
        }
    }
}
