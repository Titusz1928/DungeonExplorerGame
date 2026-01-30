using System.Collections.Generic;
using UnityEngine;

public class PlayerStartingItems : MonoBehaviour
{
    [System.Serializable]
    public struct StartingItem
    {
        public ItemSO item;
        public int amount;
    }

    public List<StartingItem> loadout;

    public void GiveItemsToPlayer(GameObject player)
    {
        Inventory inv = player.GetComponent<Inventory>();
        EquipmentManager equip = player.GetComponent<EquipmentManager>();

        if (inv == null || equip == null) return;

        // We'll keep track of what we've equipped to avoid double-equipping weapons
        bool weaponEquipped = false;
        bool shieldEquipped = false;

        foreach (var entry in loadout)
        {
            // 1. Add to inventory
            // Note: For starting items, we usually want them in the inventory first
            inv.AddItem(entry.item, entry.amount);

            // 2. Find the instance we just added to equip it
            // We look backwards from the end of the inventory to find the newest matching item
            for (int i = inv.items.Count - 1; i >= 0; i--)
            {
                ItemInstance instance = inv.items[i];

                // If this is the item we just added and it isn't equipped yet
                if (instance.itemSO == entry.item && !instance.isEquipped)
                {
                    if (instance.itemSO is ArmorItemSO)
                    {
                        equip.Equip(instance);
                    }
                    else if (instance.itemSO is WeaponItemSO && !weaponEquipped)
                    {
                        equip.Equip(instance);
                        weaponEquipped = true;
                    }
                    else if (instance.itemSO is ShieldItemSO && !shieldEquipped)
                    {
                        equip.Equip(instance);
                        shieldEquipped = true;
                    }

                    // Once we've handled this specific starting item "entry", move to the next loadout item
                    break;
                }
            }
        }
        Debug.Log("Starting items distributed and equipped.");
    }
}