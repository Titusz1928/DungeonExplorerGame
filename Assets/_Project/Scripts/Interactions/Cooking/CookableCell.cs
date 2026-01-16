using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

public class CookableCell : MonoBehaviour
{
    public Image itemIcon;
    public Image progressSliderFill;
    public Button takeButton;

    private ItemInstance linkedItem;
    private CookingStation station;

    public void Setup(ItemInstance item, CookingStation sourceStation)
    {
        linkedItem = item;
        station = sourceStation;

        if (itemIcon != null)
            itemIcon.sprite = item.itemSO.icon;

        // Reset and add the click listener
        if (takeButton != null)
        {
            takeButton.onClick.RemoveAllListeners();
            takeButton.onClick.AddListener(OnTakeClicked);
        }
    }

    private void OnTakeClicked()
    {
        if (linkedItem == null || station == null) return;

        // 1. Find the Player's Inventory via GameBoot
        if (GameBoot.PersistentPlayer == null) return;
        Inventory inventory = GameBoot.PersistentPlayer.GetComponent<Inventory>();

        if (inventory == null) return;

        // 2. Try to add 1 unit to the player inventory
        // We use the SO and a quantity of 1. 
        // This will naturally stack in the player's inventory via your AddItem logic.
        bool added = inventory.AddItem(linkedItem.itemSO, 1);

        if (added)
        {
            // 3. Success! Now remove 1 unit from the Cooking Station's container
            var container = station.GetComponent<WorldContainer>();

            container.RemoveItem(linkedItem, 1);

            // 4. Tell the station the inventory changed so the Window refreshes the grid
            station.inventoryUpdated = true;
        }
    }

    void Update()
    {
        if (linkedItem == null || progressSliderFill == null) return;

        // If the item isn't cookable, ensure the slider is hidden or 0
        if (!linkedItem.itemSO.isCookable)
        {
            // If your slider is inside a parent "SliderBackground", hide the whole thing
            if (progressSliderFill.transform.parent != null)
                progressSliderFill.gameObject.SetActive(false);
            else
                progressSliderFill.fillAmount = 0;

            return;
        }

        // Item is cookable: ensure slider is active and update fill
        if (progressSliderFill.transform.parent != null)
            progressSliderFill.transform.parent.gameObject.SetActive(true);

        float progress = linkedItem.cookingProgress / linkedItem.itemSO.cookTimeRequired;
        progressSliderFill.fillAmount = Mathf.Clamp01(progress);
    }
}