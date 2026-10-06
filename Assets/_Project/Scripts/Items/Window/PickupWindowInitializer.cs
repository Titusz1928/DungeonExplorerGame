using TitusGames.Framework;
using UnityEngine;

public class PickupWindowInitializer : MonoBehaviour
{
    [SerializeField] private GameObject pickupWindowPrefab;

    public void OpenPickupWindow()
    {
        ServiceLocator.Current.TryGet<IWindowService>(out var windowService);

        GameObject windowObj = windowService.OpenWindow(pickupWindowPrefab);

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
