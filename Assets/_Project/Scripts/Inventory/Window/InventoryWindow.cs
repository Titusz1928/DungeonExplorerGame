using UnityEngine;
using TMPro;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject titleRowPrefab;

    [SerializeField] private Transform player;
    [SerializeField] private Inventory inventory;

    private void Start()
    {
        //Refresh();
    }

    public void Initialize(Inventory inv, Transform playerTransform)
    {
        inventory = inv;
        player = playerTransform;
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
        //Debug.Log($"Drop pressed for item: {item.itemSO.itemName} (qty: {item.quantity})");

        if (inventory == null) return;

        inventory.RemoveItem(item.itemSO);
        Refresh();

        Vector3 dropPos = player.transform.position + player.transform.forward * 1.5f;

        // spawn item in the world
        ItemSpawner.Instance.SpawnWorldItem(item.itemSO, dropPos, 1);

    }
}

