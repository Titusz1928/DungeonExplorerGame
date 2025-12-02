using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupRow : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text qtyText;
    [SerializeField] private Button pickupButton;

    private WorldItem worldItem;
    private PickupWindow window;

    public void SetData(WorldItem wi, PickupWindow pw)
    {
        worldItem = wi;
        window = pw;

        nameText.text = wi.itemData.itemName;
        qtyText.text = wi.quantity.ToString();

        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(OnPickupPressed);
    }

    private void OnPickupPressed()
    {
        Inventory inv = window.GetInventory();

        inv.AddItem(worldItem.itemData, worldItem.quantity);

        Destroy(worldItem.gameObject);

        window.Refresh();
    }
}
