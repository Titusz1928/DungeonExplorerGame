using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WorldContainer : MonoBehaviour
{
    public ContainerSO containerData;

    // Actual stored items (similar structure to inventory)
    public List<ItemInstance> items = new List<ItemInstance>();


    [SerializeField] public SpriteRenderer sr;
    private Color originalColor = new Color(1f, 1f, 1f);
    public Color highlightColor = new Color(1f, 35f / 255f, 0f); // light yellow glow


    //world ID (for keeping state after unloading and reloading it)
    public Vector2Int worldCell;

    private bool initialized = false;

    public bool wasOpened = false;

    private bool isPrepopulated = false;

    public string uniqueId;

    public void Initialize(Vector2Int cell, string id)
    {
        if (initialized) return;

        worldCell = cell;

        // If id is null, we are spawning for the first time. 
        // Ask WorldSaveData for a stable ID.
        if (string.IsNullOrEmpty(id))
        {
            uniqueId = WorldSaveData.Instance.GetOrCreateContainerId(worldCell, name.Replace("(Clone)", ""));
        }
        else
        {
            uniqueId = id;
        }

        // Now check if data exists for this ID
        if (WorldSaveData.Instance.HasContainerData(uniqueId))
        {
            items = WorldSaveData.Instance.GetContainerData(uniqueId);
            wasOpened = WorldSaveData.Instance.IsContainerInitialized(uniqueId); // Restore your bool
        }
        else
        {
            if (!isPrepopulated)
            {
                GenerateContents();
            }
            // Save immediately so the ID and contents are linked
            WorldSaveData.Instance.SaveContainerData(uniqueId, items, wasOpened, initialized);
        }

        initialized = true;
    }

    // NEW FUNCTION: Call this BEFORE Initialize
    public void SetInventory(List<ItemInstance> preGeneratedItems, ContainerSO metadata)
    {
        items = new List<ItemInstance>(preGeneratedItems);
        containerData = metadata; // <--- This fixes the UI NullReference!
        isPrepopulated = true;
    }

    public void Highlight(bool on)
    {
        wasOpened = true;

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

    public void AddItemToContainer(ItemInstance instance)
    {
        // 1. Stacking Logic
        if (instance.itemSO.isStackable)
        {
            for (int i = 0; i < items.Count && instance.quantity > 0; i++)
            {
                // CHANGED: Use items[i].itemSO and items[i].quantity
                if (items[i].itemSO == instance.itemSO && items[i].quantity < instance.itemSO.maxStackSize)
                {
                    int space = instance.itemSO.maxStackSize - items[i].quantity;
                    int toAdd = Mathf.Min(space, instance.quantity);

                    items[i].quantity += toAdd;
                    instance.quantity -= toAdd;
                }
            }
        }

        // 2. If it's not stackable OR there is remainder left, add the instance
        if (instance.quantity > 0)
        {
            items.Add(instance);
        }

        // 3. Save the state
        WorldSaveData.Instance.SaveContainerData(uniqueId, items, wasOpened, initialized);
    }

    public void RemoveItem(ItemInstance instance, int qty)
    {
        // 1. Check if the instance actually exists in this container
        if (!items.Contains(instance)) return;

        // 2. Handle quantity reduction
        if (instance.quantity > qty)
        {
            instance.quantity -= qty;
        }
        else
        {
            // If qty is equal or greater, remove the whole object from the list
            items.Remove(instance);
        }

        // 3. Save the updated state of the container
        WorldSaveData.Instance.SaveContainerData(uniqueId, items, wasOpened, initialized);
    }

    public void SaveState()
    {
        WorldSaveData.Instance.SaveContainerData(uniqueId, items, wasOpened, initialized);
    }
}
