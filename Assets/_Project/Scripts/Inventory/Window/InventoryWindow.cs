using UnityEngine;
using TMPro;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

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

        foreach (ItemInstance instance in inventory.items)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            InventoryRow row = rowObj.GetComponent<InventoryRow>();
            row.SetData(instance);
        }
    }
}

