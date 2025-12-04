using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveWindow : MonoBehaviour
{
    [SerializeField] private Transform rowsParent;        // scroll view content
    [SerializeField] private GameObject rowPrefab;        // "MoveRow"
    [SerializeField] private GameObject cellPrefab;       // "MoveCell" (with button)

    private ItemInstance movingItem;
    private Transform player;
    private Inventory inventory;

    public float scanRadius = 1.0f;

    public void OpenMoveWindow(ItemInstance item, Inventory inv, Transform playerTransform)
    {
        movingItem = item;
        inventory = inv;
        player = playerTransform;

        ClearRows();
        PopulateContainerButtons();
    }

    private void ClearRows()
    {
        foreach (Transform child in rowsParent)
            Destroy(child.gameObject);
    }

    private void PopulateContainerButtons()
    {
        // find containers near the player
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, scanRadius);

        List<WorldContainer> containers = new();

        foreach (var hit in hits)
        {
            WorldContainer wc = hit.GetComponentInParent<WorldContainer>();
            if (wc != null)
                containers.Add(wc);
        }

        if (containers.Count == 0)
            return;

        Transform currentRow = Instantiate(rowPrefab, rowsParent).transform;
        int count = 0;

        foreach (var container in containers)
        {
            // start a new row every 4 cells
            if (count > 0 && count % 4 == 0)
                currentRow = Instantiate(rowPrefab, rowsParent).transform;

            GameObject cell = Instantiate(cellPrefab, currentRow);
            Button btn = cell.GetComponentInChildren<Button>();

            // assign the container sprite (from its SpriteRenderer)
            SpriteRenderer sr = container.GetComponent<SpriteRenderer>();
            if (sr != null)
                btn.image.sprite = sr.sprite;

            btn.onClick.AddListener(() => OnContainerSelected(container));

            count++;
        }
    }

    private void OnContainerSelected(WorldContainer container)
    {
        // 1) Remove 1 item from inventory
        bool removed = inventory.RemoveItem(movingItem.itemSO, 1);

        if (!removed)
        {
            Debug.LogWarning("Could not remove item from inventory!");
            CloseWindow();
            return;
        }

        // 2) Add 1 item to the container
        container.AddItemToContainer(movingItem.itemSO, 1);

        // 3) Refresh the inventory UI
        InventoryWindow invWindow = FindObjectOfType<InventoryWindow>();
        if (invWindow != null)
            invWindow.Refresh();

        // 4) Close window
        CloseWindow();
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}
