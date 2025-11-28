using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button dropButton;

    public void SetData(ItemInstance instance)
    {
        idText.text = instance.itemSO.ID.ToString();
        nameText.text = instance.itemSO.itemName;
        typeText.text = instance.itemSO.category.ToString();
        durabilityText.text = instance.currentDurability.ToString();
        amountText.text = instance.quantity.ToString();
    }
}
