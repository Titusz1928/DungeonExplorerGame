using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupRow : MonoBehaviour
{
    //[SerializeField] private TMP_Text nameText;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text qtyText;
    [SerializeField] private Button pickupButton;

    private WorldContainer sourceContainer; //for switching to the correct container after refreshing (for example when picking up an item)

    private WorldItem worldItem;                 // for ground
    private ItemInstance linkedInstance;  // For container (FIXED: Added this field)
    private PickupWindow window;

    private ItemSO containerItemSO;              // for container
    private int containerItemQty;
    private WorldContainer parentContainer;      // reference to container

    private bool isFromGround = true;

    // ----------------------------------------------------------
    // OVERLOAD 1: For items lying on the 2D Floor
    // ----------------------------------------------------------
    public void SetData(WorldItem wi, PickupWindow pw)
    {
        isFromGround = true;
        worldItem = wi;
        linkedInstance = null; // Clear the other reference
        window = pw;
        sourceContainer = null;

        //nameText.text = wi.itemSO.itemName;
        itemImage.sprite = wi.itemSO.icon;
        qtyText.text = wi.quantity.ToString();

        SetupButton();
    }

    private void SetupButton()
    {
        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(OnPickupPressed);
    }

    // ----------------------------------------------------------
    // OVERLOAD 2: For items inside a Chest/Container
    // ----------------------------------------------------------
    public void SetData(ItemInstance instance, WorldContainer container, PickupWindow pw)
    {
        isFromGround = false;
        linkedInstance = instance;
        worldItem = null; // Clear the other reference
        parentContainer = container;
        sourceContainer = container;
        window = pw;

        //nameText.text = instance.itemSO.itemName;
        itemImage.sprite = instance.itemSO.icon;
        qtyText.text = instance.quantity.ToString();

        SetupButton();
    }

    // ----------------------------------------------------------
    // PICKUP LOGIC
    // ----------------------------------------------------------
    private void OnPickupPressed()
    {
        Inventory inv = window.GetInventory();
        bool added = false;

        if (isFromGround)
        {
            // GROUND LOGIC: Create a new instance from WorldItem data
            ItemInstance instanceToPickUp = new ItemInstance(worldItem.itemSO, worldItem.quantity);
            instanceToPickUp.currentDurability = worldItem.currentDurability;

            added = inv.AddItemInstance(instanceToPickUp);

            if (added)
            {
                // 2. Reduce the quantity on the ground
                worldItem.quantity -= 1;

                // 3. If no more items are left, destroy the physical object
                if (worldItem.quantity <= 0)
                {
                    Destroy(worldItem.gameObject); // Important: Destroy the GameObject, not the script
                }
            }
        }
        else
        {
            // CONTAINER LOGIC: Use the exact ItemInstance reference from the container
            // We use AddItemInstance to ensure durability/holes are preserved
            added = inv.AddItemInstance(linkedInstance);

            if (added)
            {
                // We tell the container to remove this specific instance
                // We pass 1 because typically PickupRow handles items one by one
                parentContainer.RemoveItem(linkedInstance, 1);
            }
        }

        if (added)
        {
            window.Refresh();

            // Return to the view the player was just looking at
            if (isFromGround || sourceContainer == null)
                window.ShowGroundItems();
            else
                window.ShowContainerItems(sourceContainer);
        }
    }
}
