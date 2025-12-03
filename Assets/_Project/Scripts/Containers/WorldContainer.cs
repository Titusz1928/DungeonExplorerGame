using UnityEngine;
using System.Collections.Generic;

public class WorldContainer : MonoBehaviour
{
    public ContainerSO containerData;

    // Actual stored items (similar structure to inventory)
    public List<(ItemSO item, int qty)> items = new();

    private bool initialized = false;

    private void Start()
    {
        if (!initialized) GenerateContents();
    }

    private void GenerateContents()
    {
        if (containerData == null || containerData.allowedItems.Count == 0)
            return;

        int count = Random.Range(containerData.minItems, containerData.maxItems + 1);

        for (int i = 0; i < count; i++)
        {
            ItemSO item = containerData.allowedItems[Random.Range(0, containerData.allowedItems.Count)];
            int qty = 1; // or random if needed

            items.Add((item, qty));
        }

        initialized = true;
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
