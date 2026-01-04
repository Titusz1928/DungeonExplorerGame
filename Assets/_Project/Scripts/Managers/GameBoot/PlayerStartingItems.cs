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
        if (inv == null) return;

        foreach (var entry in loadout)
        {
            inv.AddItem(entry.item, entry.amount);
        }
        Debug.Log("Starting items distributed to player.");
    }
}