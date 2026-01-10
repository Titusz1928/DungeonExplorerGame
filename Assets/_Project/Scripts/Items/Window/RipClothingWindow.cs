using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RipClothingWindow : MonoBehaviour
{
    [SerializeField] private Transform rowsParent;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private TextMeshProUGUI headerText;

    [SerializeField] private AudioClip ripClothingAudio;

    private ItemInstance scissorInstance;
    private Inventory inventory;

    public void OpenRipWindow(ItemInstance scissors, Inventory inv)
    {
        scissorInstance = scissors;
        inventory = inv;
        //headerText.text = "Select Clothing to Rip";

        ClearRows();
        PopulateClothingList();
    }

    private void ClearRows()
    {
        foreach (Transform child in rowsParent)
            Destroy(child.gameObject);
    }

    private void PopulateClothingList()
    {
        // Filter for Armor items with the "Under" layer
        List<ItemInstance> underClothing = inventory.items.FindAll(item =>
        item.itemSO is ArmorItemSO armor &&
        armor.layer == ArmorLayer.Under &&
        !item.isEquipped);

        if (underClothing.Count == 0)
        {
            headerText.text = "No 'Under' clothing found!";
            return;
        }

        Transform currentRow = Instantiate(rowPrefab, rowsParent).transform;
        int count = 0;

        foreach (var clothing in underClothing)
        {
            if (count > 0 && count % 4 == 0)
                currentRow = Instantiate(rowPrefab, rowsParent).transform;

            GameObject cell = Instantiate(cellPrefab, currentRow);
            Button btn = cell.GetComponentInChildren<Button>();

            // Set the icon of the clothing piece
            btn.image.sprite = clothing.itemSO.icon;

            // Add listener to rip this specific clothing instance
            btn.onClick.AddListener(() => OnClothingSelected(clothing));

            count++;
        }
    }

    private void OnClothingSelected(ItemInstance clothingToRip)
    {
        // 1. Remove the clothing item (1 quantity)
        bool removed = inventory.RemoveItem(clothingToRip, 1);
        if (!removed) return;

        // 2. Add 3 Ripped Clothing (Bandages) - ID 17
        ItemSO bandageSO = ItemDatabase.instance.GetByID(18);
        if (bandageSO != null)
        {
            inventory.AddItem(bandageSO, 3);
        }

        // 3. Decrease Scissor Durability
        scissorInstance.currentDurability -= 1;

        // Check if scissors broke
        if (scissorInstance.currentDurability <= 0)
        {
            MessageManager.Instance.ShowMessageDirectly("The scissors have broken!");
        }
        else
        {
            MessageManager.Instance.ShowMessageDirectly("Created 3 Ripped Clothing");
        }

        AudioManager.Instance.PlaySFX(ripClothingAudio);

        // 4. Refresh and Close
        InventoryWindow invWindow = FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null) invWindow.Refresh();

        CloseWindow();
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}