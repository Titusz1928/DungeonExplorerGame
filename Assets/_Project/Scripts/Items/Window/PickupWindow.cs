using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PickupWindow : MonoBehaviour
{
    [Header("Item Rows")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

    [Header("Ground Tab")]
    [SerializeField] private ContainerTab groundTab;
    [SerializeField] private Sprite groundIcon;

    [Header("Container Tabs")]
    [SerializeField] private Transform containerTabContainer;
    [SerializeField] private GameObject containerTabPrefab;

    private Transform player;
    private Inventory playerInventory;

    public float scanRadius = 1.0f;

    private WorldContainer activeContainer;

    // -------------------------------
    // INITIALIZATION
    // -------------------------------
    public void Initialize(Inventory inv, Transform playerTransform)
    {
        playerInventory = inv;
        player = playerTransform;

        if (!player) Debug.LogError("PickupWindow: Player is NULL!");
        if (!playerInventory) Debug.LogError("PickupWindow: Inventory is NULL!");

        // Setup ground tab (permanent)
        if (groundTab != null)
            groundTab.SetGroundTab(this, groundIcon);

        Refresh();
        ShowGroundItems();
    }

    public Inventory GetInventory() => playerInventory;

    // -------------------------------
    // REFRESH (REBUILD TABS)
    // -------------------------------
    public void Refresh()
    {
        if (!player) return;

        // Remove ONLY the dynamic container tabs
        foreach (Transform child in containerTabContainer)
        {
            if (child.gameObject != groundTab.gameObject)
                Destroy(child.gameObject);
        }

        // Scan for world containers
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, scanRadius);
        Debug.Log("Hit count = " + hits.Length);

        foreach (var hit in hits)
        {
            Debug.Log("Hit object: " + hit.name);

            WorldContainer wc = hit.GetComponent<WorldContainer>();
            if (wc != null)
            {
                Debug.Log("FOUND container: " + hit.name);

                // create a tab for each container
                GameObject tabObj = Instantiate(containerTabPrefab, containerTabContainer);
                Debug.Log("Created container tab: " + tabObj.name);

                ContainerTab tab = tabObj.GetComponent<ContainerTab>();
                tab.SetContainerData(wc, this);
            }
            else
            {
                Debug.Log("NOT a container: " + hit.name);
            }
        }

        Debug.Log("---- REFRESH END ----");
    }

    // -------------------------------
    // SHOW GROUND ITEMS
    // -------------------------------
    public void ShowGroundItems()
    {
        ClearRows();

        if (activeContainer != null)
            activeContainer.Highlight(false);

        ScanAndDisplayGroundItems();
    }

    private void ScanAndDisplayGroundItems()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, scanRadius);

        List<WorldItem> items = new List<WorldItem>();

        foreach (var hit in hits)
        {
            WorldItem wi = hit.GetComponent<WorldItem>();
            if (wi != null)
                items.Add(wi);
        }

        foreach (var wi in items)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            PickupRow row = rowObj.GetComponent<PickupRow>();
            row.SetData(wi, this);
        }
    }

    // -------------------------------
    // SHOW CONTAINER ITEMS
    // -------------------------------
    public void ShowContainerItems(WorldContainer wc)
    {
        ClearRows();

        // Remove highlight from previous
        if (activeContainer != null)
            activeContainer.Highlight(false);

        // Highlight new
        activeContainer = wc;

        if (activeContainer != null)
        {
            Debug.Log("Highlighting container: " + wc.name);
            activeContainer.Highlight(true);
        }

        // UPDATED: 'entry' is now an ItemInstance object
        foreach (ItemInstance inst in wc.items)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            PickupRow row = rowObj.GetComponent<PickupRow>();

            // CHANGED: Pass the instance directly. 
            // This matches the new SetData(ItemInstance, WorldContainer, PickupWindow) signature.
            row.SetData(inst, wc, this);
        }
    }

    // -------------------------------
    // UTILITIES
    // -------------------------------
    private void ClearRows()
    {
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);
    }

    private void OnDestroy()
    {
        // Safety: also handle destroy case
        if (activeContainer != null)
        {
            activeContainer.Highlight(false);
            activeContainer = null;
        }
    }
}
