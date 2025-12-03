using UnityEngine;
using System.Collections.Generic;

public class WorldContainer : MonoBehaviour
{
    public ContainerSO containerData;

    // Actual stored items (similar structure to inventory)
    public List<(ItemSO item, int qty)> items = new();

    private bool initialized = false;

    [SerializeField] private SpriteRenderer sr;
    private Color originalColor = new Color(1f, 1f, 1f);
    public Color highlightColor = new Color(1f, 35f / 255f, 0f); // light yellow glow


    private void Start()
    {
        if (!initialized) GenerateContents();
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
        if (containerData == null || containerData.allowedItems.Count == 0)
            return;

        int count = Random.Range(containerData.minItems, containerData.maxItems + 1);

        for (int i = 0; i < count; i++)
        {
            ItemSO item = containerData.allowedItems[Random.Range(0, containerData.allowedItems.Count)];

            int qty = item.isStackable
                ? Random.Range(1, item.maxStackSize + 1) // optional randomness
                : 1;

            AddItemToContainer(item, qty);
        }

        initialized = true;
    }

    private void AddItemToContainer(ItemSO item, int amount)
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

                return;
            }
        }
    }
}
