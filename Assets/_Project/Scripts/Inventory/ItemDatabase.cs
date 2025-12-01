using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    public List<ItemSO> allItems;

    private Dictionary<int, ItemSO> lookup;

    void Awake()
    {
        instance = this;

        if (allItems == null || allItems.Count == 0)
            allItems = new List<ItemSO>(Resources.LoadAll<ItemSO>("Items"));

        lookup = new Dictionary<int, ItemSO>();
        foreach (var item in allItems)
            lookup[item.ID] = item;
    }

    public ItemSO GetByID(int id)
    {
        return lookup[id];
    }
}
