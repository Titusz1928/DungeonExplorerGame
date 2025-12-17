using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WorldContainer : MonoBehaviour
{
    public ContainerSO containerData;

    // Actual stored items (similar structure to inventory)
    public List<(ItemSO item, int qty)> items = new();


    [SerializeField] public SpriteRenderer sr;
    private Color originalColor = new Color(1f, 1f, 1f);
    public Color highlightColor = new Color(1f, 35f / 255f, 0f); // light yellow glow


    //world ID (for keeping state after unloading and reloading it)
    public Vector2Int worldCell;

    private bool initialized = false;

    public string uniqueId;

    public void Initialize(Vector2Int cell, string id)
    {
        if (initialized)
            return;

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError($"[WorldContainer] Missing uniqueId on {name}");
            return;
        }

        worldCell = cell;
        uniqueId = id;

        if (WorldSaveData.Instance.HasContainerData(uniqueId))
        {
            items = WorldSaveData.Instance.GetContainerData(uniqueId);
        }
        else
        {
            Debug.Log("[WORLD CONTAINER] generating items for:" + id);
            GenerateContents();
            WorldSaveData.Instance.SaveContainerData(uniqueId, items);
        }

        initialized = true;
    }

    public void Highlight(bool on)
    {
        if (sr == null)
        {
            Debug.Log("early return");
            return;
        }
        Debug.Log("no early return");
        sr.color = on ? highlightColor : originalColor;
    }

    private void GenerateContents()
    {
        if (containerData == null)
            return;

        items = containerData.GenerateLoot();

    }

    public void AddItemToContainer(ItemSO item, int amount)
    {
        // Merge into existing stacks
        if (item.isStackable)
        {
            for (int i = 0; i < items.Count && amount > 0; i++)
            {
                if (items[i].item == item && items[i].qty < item.maxStackSize)
                {
                    int space = item.maxStackSize - items[i].qty;
                    int toAdd = Mathf.Min(space, amount);

                    items[i] = (item, items[i].qty + toAdd);
                    amount -= toAdd;
                }
            }
        }

        // Create new stacks
        while (amount > 0)
        {
            int stackAmount = item.isStackable
                ? Mathf.Min(item.maxStackSize, amount)
                : 1;

            items.Add((item, stackAmount));
            amount -= stackAmount;
        }

        WorldSaveData.Instance.SaveContainerData(uniqueId,items);
    }

    public void RemoveItem(ItemSO item, int qty)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == item)
            {
                var entry = items[i];

                // Reduce quantity
                entry.qty -= qty;

                if (entry.qty <= 0)
                {
                    // Remove entry entirely
                    items.RemoveAt(i);
                }
                else
                {
                    // Save updated quantity back
                    items[i] = entry;
                }

                WorldSaveData.Instance.SaveContainerData(uniqueId, items);
                return;
            }
        }
    }
}
