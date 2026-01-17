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
            Debug.Log(items.Count + " < " + maxSlots);
            int addAmount = itemSO.isStackable ? Mathf.Min(itemSO.maxStackSize, amount) : 1;

            items.Add(new ItemInstance(itemSO, addAmount));
            amount -= addAmount;
        }

        return amount <= 0;
    }

    public bool AddItemInstance(ItemInstance instance, int amount = 1)
    {
        if (instance.itemSO.isStackable)
        {
            // Try to add/merge only the specified amount
            return AddItem(instance.itemSO, amount);
        }

        // For non-stackable items, we add the instance itself
        if (items.Count < maxSlots)
        {
            // If we are moving a non-stackable item, we usually move the whole instance
            items.Add(instance);
            return true;
        }

        return false;
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

    // --- CRAFTING HELPERS ---

    /// <summary>
    /// Checks if the inventory contains the total required quantity for all ingredients.
    /// </summary>
    public bool HasIngredients(List<IngredientSlot> ingredients)
    {
        foreach (var req in ingredients)
        {
            int totalFound = 0;
            foreach (var item in items)
            {
                if (item.itemSO == req.item)
                {
                    totalFound += item.quantity;
                }
            }

            if (totalFound < req.quantity)
            {
                return false; // Missing enough of this specific ingredient
            }
        }
        return true;
    }

    /// <summary>
    /// Deducts the specified ingredients from the inventory. 
    /// Should only be called AFTER HasIngredients returns true.
    /// </summary>
    public void RemoveIngredients(List<IngredientSlot> ingredients)
    {
        foreach (var req in ingredients)
        {
            int amountToRemove = req.quantity;

            // Iterate backwards so we can safely remove empty stacks from the list
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].itemSO == req.item)
                {
                    if (items[i].quantity > amountToRemove)
                    {
                        // This stack has more than we need; just subtract
                        items[i].quantity -= amountToRemove;
                        amountToRemove = 0;
                    }
                    else
                    {
                        // This stack is exactly what we need or less; remove the whole stack
                        amountToRemove -= items[i].quantity;
                        items.RemoveAt(i);
                    }
                }

                if (amountToRemove <= 0) break; // We found enough for this ingredient
            }
        }
    }
}
