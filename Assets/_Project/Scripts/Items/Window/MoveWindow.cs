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
        // 1) Handle Stack Logic vs Single Item Logic
        if (movingItem.quantity > 1)
        {
            // If it's a stack, we create a "New Instance" for the 1 item being moved
            ItemInstance oneFromStack = new ItemInstance(movingItem.itemSO, 1);
            oneFromStack.currentDurability = movingItem.currentDurability;

            // Add the new 1-count instance to container
            container.AddItemToContainer(oneFromStack);

            // Remove only 1 from the player's inventory stack
            inventory.RemoveItem(movingItem, 1);
        }
        else
        {
            // If it's a single item (like Scissors), we move the WHOLE instance
            // This preserves the EXACT object reference and its durability
            bool removed = inventory.RemoveItem(movingItem, 1);

            if (removed)
            {
                container.AddItemToContainer(movingItem);
            }
            else
            {
                Debug.LogWarning("Could not remove item from inventory!");
                CloseWindow();
                return;
            }
        }

        // 2) Refresh and Close
        InventoryWindow invWindow = FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null)
            invWindow.Refresh();

        CloseWindow();
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}
