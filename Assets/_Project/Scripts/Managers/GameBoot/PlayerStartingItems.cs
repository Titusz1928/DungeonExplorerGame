using System.Collections.Generic;
using TitusGames.Framework;
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
        if (player == null)
        {
            Debug.LogError("[PlayerStartingItems] Player GameObject is null!");
            return;
        }

        Inventory inv = player.GetComponent<Inventory>();

        // Try getting IEquipmentService from player GameObject, fallback to ServiceLocator
        if (!player.TryGetComponent<IEquipmentService>(out var equip))
        {
            ServiceLocator.Current.TryGet<IEquipmentService>(out equip);
        }

        if (inv == null || equip == null)
        {
            Debug.LogError($"[PlayerStartingItems] Required components missing! Inventory null: {inv == null}, EquipmentService null: {equip == null}");
            return;
        }

        bool weaponEquipped = false;
        bool shieldEquipped = false;

        foreach (var entry in loadout)
        {
            if (entry.item == null || entry.amount <= 0) continue;

            // 1. Add to inventory
            inv.AddItem(entry.item, entry.amount);

            // 2. Find the instance we just added to equip it
            for (int i = inv.items.Count - 1; i >= 0; i--)
            {
                ItemInstance instance = inv.items[i];

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

                    break;
                }
            }
        }

        Debug.Log("[PlayerStartingItems] Starting items distributed and equipped.");
    }
}