using UnityEngine;

public class InventoryWindowInitializer : MonoBehaviour
{
    [SerializeField] private GameObject inventoryWindowPrefab; // assign the prefab here

    public void OpenInventoryWindow()
    {
        // Open window via WindowManager
        GameObject windowObj = WindowManager.Instance.OpenWindow(inventoryWindowPrefab);

        // Get InventoryWindow component
        InventoryWindow invWindow = windowObj.GetComponent<InventoryWindow>();
        invWindow.SetInventory(FindObjectOfType<Inventory>());
    }
}

