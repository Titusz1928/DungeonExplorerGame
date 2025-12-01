using UnityEngine;

public class InventoryWindowInitializer : MonoBehaviour
{
    [SerializeField] private GameObject inventoryWindowPrefab;

    public void OpenInventoryWindow()
    {
        GameObject windowObj = WindowManager.Instance.OpenWindow(inventoryWindowPrefab);

        InventoryWindow invWindow = windowObj.GetComponent<InventoryWindow>();

        Inventory inventory = FindObjectOfType<Inventory>();
        Transform player = inventory.transform;   // player is the one who has the Inventory

        invWindow.Initialize(inventory, player);
    }
}

