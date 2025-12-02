using UnityEngine;
using System.Collections.Generic;

public class PickupWindow : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

    private Transform player;
    private Inventory playerInventory;

    public float scanRadius = 3.0f;

    public void Initialize(Inventory inv, Transform playerTransform)
    {
        Debug.Log("PickupWindow.Initialize called");

        playerInventory = inv;
        player = playerTransform;

        if (player == null) Debug.LogError("Player IS NULL in Initialize!");
        if (playerInventory == null) Debug.LogError("Inventory IS NULL in Initialize!");

        Refresh();
    }

    public Inventory GetInventory()
    {
        return playerInventory;
    }

    public void Refresh()
    {
        Debug.Log("PickupWindow.Refresh called");

        if (player == null || playerInventory == null)
        {
            Debug.LogError("PickupWindow missing player or inventory!");
            return;
        }

        if (rowContainer == null)
        {
            Debug.LogError("rowContainer is NULL!");
            return;
        }

        Debug.Log("Clearing previous rows... childCount = " + rowContainer.childCount);

        // Clear all old rows
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        Debug.Log("Rows cleared. New scan starting...");

        // Search for items
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, scanRadius);
        Debug.Log("OverlapCircle hit count: " + hits.Length);

        foreach (var hit in hits)
        {
            Debug.Log("Hit object: " + hit.name + "   Has WorldItem = " + (hit.GetComponent<WorldItem>() != null));
        }

        List<WorldItem> items = new List<WorldItem>();

        foreach (var hit in hits)
        {
            WorldItem wi = hit.GetComponent<WorldItem>();
            if (wi != null)
            {
                Debug.Log("Found world item: " + wi.itemData.itemName);
                items.Add(wi);
            }
        }

        Debug.Log("Total world items found: " + items.Count);

        // Create rows
        foreach (var wi in items)
        {
            Debug.Log("Creating row for: " + wi.itemData.itemName);

            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            PickupRow row = rowObj.GetComponent<PickupRow>();
            row.SetData(wi, this);
        }

        Debug.Log("Refresh COMPLETED — Total rows created: " + rowContainer.childCount);
    }
}
