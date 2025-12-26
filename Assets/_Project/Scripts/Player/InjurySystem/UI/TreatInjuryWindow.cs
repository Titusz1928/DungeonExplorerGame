using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreatInjuryWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform rowsParent;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private TextMeshProUGUI headerText;

    private ArmorSlot targetLocation;
    private Inventory playerInventory;
    private Injury targetInjury;

    public void OpenTreatWindow(Injury injury)
    {
        targetInjury = injury;
        headerText.text = $"Treating: {injury.type} on {injury.bodyPart}";

        playerInventory = PlayerStateManager.Instance.GetComponent<Inventory>();

        ClearRows();
        PopulateMedicalItems();
    }

    private void ClearRows()
    {
        foreach (Transform child in rowsParent)
            Destroy(child.gameObject);
    }

    private void PopulateMedicalItems()
    {

        List<ItemInstance> items = playerInventory.items;
        List<ItemInstance> medicalItems = new();

        // Filter for Potion and FirstAid
        foreach (var item in items)
        {
            if (item.itemSO is ConsumableItemSO consumable)
            {
                if (consumable.consumableType == ConsumableType.Potion ||
                    consumable.consumableType == ConsumableType.FirstAid)
                {
                    medicalItems.Add(item);
                }
            }
        }

        if (medicalItems.Count == 0) return;

        Transform currentRow = Instantiate(rowPrefab, rowsParent).transform;
        int count = 0;

        foreach (var item in medicalItems)
        {
            // Start a new row every 4 items
            if (count > 0 && count % 4 == 0)
                currentRow = Instantiate(rowPrefab, rowsParent).transform;

            GameObject cell = Instantiate(cellPrefab, currentRow);
            Button btn = cell.GetComponentInChildren<Button>();

            // Set icon and optional quantity text
            btn.image.sprite = item.itemSO.icon;

            // If your cell has a TMP text for amount
            TextMeshProUGUI amountText = cell.GetComponentInChildren<TextMeshProUGUI>();
            if (amountText != null) amountText.text = item.quantity.ToString();

            btn.onClick.AddListener(() => OnItemSelected(item));
            count++;
        }
    }

    private void OnItemSelected(ItemInstance item)
    {
        ConsumableItemSO consumable = (ConsumableItemSO)item.itemSO;
        InjuryManager manager = PlayerStateManager.Instance.GetComponent<InjuryManager>();

        if (consumable.consumableType == ConsumableType.Potion)
        {
            // Pass the reference directly
            manager.RemoveInjury(targetInjury);
            PlayerStateManager.Instance.addHealth(consumable.healthAmount);
        }
        else if (consumable.consumableType == ConsumableType.FirstAid)
        {
            // Pass the reference directly
            manager.ApplyBandage(targetInjury);
        }

        playerInventory.RemoveItem(item, 1);

        InventoryWindow invWindow = FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null) invWindow.Refresh();

        CloseWindow();
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}