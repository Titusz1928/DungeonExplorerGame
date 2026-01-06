using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Added the interface implementation here
public class ConsumableRowPrefab : MonoBehaviour, IItemRow
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button useButton;

    private ItemInstance linkedItem;

    // This now correctly satisfies the IItemRow interface
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
        ConsumableUseHandler.UseItem(linkedItem);
    }
}