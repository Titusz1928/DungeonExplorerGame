using UnityEngine;
using TMPro;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject titleRowPrefab;

    [SerializeField] private Inventory inventory;   // <-- Assign this in Inspector

    private void Start()
    {
        //Refresh();
    }

    public void SetInventory(Inventory inv)
    {
        inventory = inv;
        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory not assigned!");
            return;
        }

        if (rowContainer == null || rowPrefab == null)
        {
            Debug.LogError("Row container or row prefab not assigned!");
            return;
        }

        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);


        if (titleRowPrefab != null)
        {
            Instantiate(titleRowPrefab, rowContainer);
        }
        else
        {
            Debug.LogWarning("titleRowPrefab not assigned! No title row will be created.");
        }

        foreach (ItemInstance instance in inventory.items)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            InventoryRow row = rowObj.GetComponent<InventoryRow>();
            row.SetData(instance, this);
        }
    }

    public void OnDropButtonPressed(ItemInstance item)
    {
        Debug.Log($"Drop pressed for item: {item.itemSO.itemName} (qty: {item.quantity})");
    }
}

