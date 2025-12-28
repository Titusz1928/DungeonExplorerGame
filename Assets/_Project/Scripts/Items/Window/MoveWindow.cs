using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform conatinerScrollField;
    [SerializeField] private Transform rowsParent;        // scroll view content
    [SerializeField] private GameObject rowPrefab;        // "MoveRow"
    [SerializeField] private GameObject cellPrefab;       // "MoveCell" (with button)

    [Header("Slider Settings")]
    [SerializeField] private GameObject sliderContainer; // The parent object of the slider UI
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private TextMeshProUGUI quantityText; // To show "5 / 10"

    private ItemInstance movingItem;
    private Transform player;
    private Inventory inventory;

    public float scanRadius = 1.0f;

    public void OpenMoveWindow(ItemInstance item, Inventory inv, Transform playerTransform)
    {
        movingItem = item;
        inventory = inv;
        player = playerTransform;

        SetupSlider();
        ClearRows();
        PopulateContainerButtons();
    }

    private void SetupSlider()
    {
        if (movingItem.quantity > 1)
        {
            sliderContainer.SetActive(true);

            // Set Slider Values
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = movingItem.quantity;
            quantitySlider.value = movingItem.quantity; // Default to moving the whole stack

            // Adjust RowsParent Anchors (Shrink to make room for slider)
            conatinerScrollField.anchorMax = new Vector2(conatinerScrollField.anchorMax.x, 0.7f);

            UpdateQuantityText();
        }
        else
        {
            sliderContainer.SetActive(false);

            // Adjust RowsParent Anchors (Fill the whole space)
            conatinerScrollField.anchorMax = new Vector2(conatinerScrollField.anchorMax.x, 1.0f);
        }

        // Reset offsets to 0 so the anchors take full effect
        conatinerScrollField.offsetMax = Vector2.zero;
        conatinerScrollField.offsetMin = Vector2.zero;
    }

    // --- Slider & Button Logic ---

    public void OnSliderValueChanged(float value)
    {
        UpdateQuantityText();
    }

    public void AdjustQuantity(int amount)
    {
        quantitySlider.value += amount; // Slider component handles clamping automatically
        UpdateQuantityText();
    }

    private void UpdateQuantityText()
    {
        if (quantityText != null)
            quantityText.text = $"{quantitySlider.value} / {movingItem.quantity}";
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
        int amountToMove = movingItem.quantity > 1 ? (int)quantitySlider.value : 1;

        // 1) Handle Stack Logic
        if (amountToMove < movingItem.quantity)
        {
            // Partial move: Create a copy for the container, subtract from inventory
            ItemInstance partialStack = new ItemInstance(movingItem.itemSO, amountToMove);
            partialStack.currentDurability = movingItem.currentDurability;

            container.AddItemToContainer(partialStack);
            inventory.RemoveItem(movingItem, amountToMove);
        }
        else
        {
            // Full move: Move the whole instance (preserves durability/refs)
            bool removed = inventory.RemoveItem(movingItem, amountToMove);
            if (removed)
            {
                container.AddItemToContainer(movingItem);
            }
        }

        // 2) Refresh and Close
        InventoryWindow invWindow = Object.FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null) invWindow.Refresh();

        CloseWindow();
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}
