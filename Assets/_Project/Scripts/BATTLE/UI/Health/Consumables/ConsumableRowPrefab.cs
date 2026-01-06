using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableRowPrefab : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button useButton;

    private ItemInstance linkedItem;

    public void SetData(ItemInstance instance)
    {
        linkedItem = instance;

        iconImage.sprite = instance.itemSO.icon;
        amountText.text = "x" + instance.quantity.ToString();

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(OnUseClicked);
    }

    private void OnUseClicked()
    {
        // Use the item via a centralized logic (we'll define this below)
        ConsumableUseHandler.UseItem(linkedItem);
    }
}