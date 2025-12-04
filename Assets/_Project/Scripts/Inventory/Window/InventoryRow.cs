using TMPro;
//using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRow : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button dropButton;

    private ItemInstance linkedItem;         // reference to the actual item
    private InventoryWindow parentWindow;    // reference to the UI window

    public void SetData(ItemInstance instance, InventoryWindow window)
    {
        linkedItem = instance;
        parentWindow = window;

        //idText.text = instance.itemSO.ID.ToString();
        nameText.text = instance.itemSO.itemName;
        typeText.text = instance.itemSO.category.ToString();
        durabilityText.text = instance.currentDurability.ToString();
        amountText.text = instance.quantity.ToString();

        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(OnMovePressed);

        dropButton.onClick.RemoveAllListeners();
        dropButton.onClick.AddListener(OnDropPressed);
    }

    private void OnMovePressed()
    {
        parentWindow.OnMoveButtonPressed(linkedItem);
    }

    private void OnDropPressed()
    {
        parentWindow.OnDropButtonPressed(linkedItem);
    }
}
