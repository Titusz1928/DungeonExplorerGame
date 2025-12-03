using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupRow : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text qtyText;
    [SerializeField] private Button pickupButton;

    private WorldItem worldItem;                 // for ground
    private PickupWindow window;

    private ItemSO containerItemSO;              // for container
    private int containerItemQty;
    private WorldContainer parentContainer;      // reference to container

    private bool isFromGround = true;

    // ----------------------------------------------------------
    // GROUND ITEMS
    // ----------------------------------------------------------
    public void SetData(WorldItem wi, PickupWindow pw)
    {
        isFromGround = true;

        worldItem = wi;
        window = pw;

        nameText.text = wi.itemData.itemName;
        qtyText.text = wi.quantity.ToString();

        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(OnPickupPressed);
    }

    // ----------------------------------------------------------
    // CONTAINER ITEMS
    // ----------------------------------------------------------
    public void SetData(ItemSO item, int qty, WorldContainer container, PickupWindow pw)
    {
        isFromGround = false;

        containerItemSO = item;
        containerItemQty = qty;
        parentContainer = container;
        window = pw;

        nameText.text = item.itemName;
        qtyText.text = qty.ToString();

        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(OnPickupPressed);
    }

    // ----------------------------------------------------------
    // PICKUP LOGIC
    // ----------------------------------------------------------
    private void OnPickupPressed()
    {
        Inventory inv = window.GetInventory();

        if (isFromGround)
        {
            inv.AddItem(worldItem.itemData, worldItem.quantity);
            Destroy(worldItem.gameObject);
        }
        else
        {
            inv.AddItem(containerItemSO, containerItemQty);

            // remove from container
            parentContainer.RemoveItem(containerItemSO, containerItemQty);
        }

        window.Refresh();
        window.ShowGroundItems();
    }
}
