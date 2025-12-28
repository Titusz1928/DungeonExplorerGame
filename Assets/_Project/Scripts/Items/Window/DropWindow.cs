using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private TextMeshProUGUI quantityText;

    private ItemInstance movingItem;
    private Inventory inventory;
    private Transform player;

    public void OpenDropWindow(ItemInstance item, Inventory inv, Transform playerTransform)
    {
        movingItem = item;
        inventory = inv;
        player = playerTransform;

        // Configure Slider (Always at least 2 to reach here based on your logic)
        quantitySlider.minValue = 1;
        quantitySlider.maxValue = movingItem.quantity;
        quantitySlider.value = movingItem.quantity; // Start at 1 as requested previously

        UpdateQuantityText();
    }

    public void OnSliderValueChanged(float value)
    {
        UpdateQuantityText();
    }

    public void AdjustQuantity(int amount)
    {
        quantitySlider.value += amount;
        UpdateQuantityText();
    }

    private void UpdateQuantityText()
    {
        if (quantityText != null)
            quantityText.text = $"{(int)quantitySlider.value} / {movingItem.quantity}";
    }

    public void ConfirmDrop()
    {
        int amountToDrop = (int)quantitySlider.value;

        // 1) HANDLE UNEQUIP LOGIC
        // If we drop the whole stack and it's equipped, we must unequip it
        if (movingItem.isEquipped && amountToDrop >= movingItem.quantity)
        {
            switch (movingItem.itemSO)
            {
                case ShieldItemSO shield: EquipmentManager.Instance.UnequipShield(movingItem); break;
                case ArmorItemSO armor: EquipmentManager.Instance.UnequipArmor(movingItem); break;
                case WeaponItemSO weapon: EquipmentManager.Instance.UnequipMainHand(movingItem); break;
            }
        }

        // 2) PREPARE DROP DATA
        Vector3 spawnPos = player.position;

        // Create a temporary instance to pass to the spawner for the correct quantity
        ItemInstance dropInstance = new ItemInstance(movingItem.itemSO, amountToDrop);
        dropInstance.currentDurability = movingItem.currentDurability;

        // 3) SPAWN AND REMOVE
        ItemSpawner.Instance.SpawnWorldItem(dropInstance, spawnPos);
        inventory.RemoveItem(movingItem, amountToDrop);

        // 4) REFRESH AND CLOSE
        InventoryWindow invWindow = Object.FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null) invWindow.Refresh();

        CloseWindow();
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}