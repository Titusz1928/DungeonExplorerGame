using UnityEngine;

public class PickupWindowInitializer : MonoBehaviour
{
    [SerializeField] private GameObject pickupWindowPrefab;

    public void OpenPickupWindow()
    {
        GameObject windowObj = WindowManager.Instance.OpenWindow(pickupWindowPrefab);

        PickupWindow pw = windowObj.GetComponent<PickupWindow>();

        // Exactly like InventoryWindowInitializer:
        Inventory inventory = FindObjectOfType<Inventory>();
        Transform player = inventory.transform;

        if (inventory == null || player == null)
        {
            Debug.LogError("PickupWindowInitializer: Could not find player or inventory!");
            return;
        }

        pw.Initialize(inventory, player);
    }
}
