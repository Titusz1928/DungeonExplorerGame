using TMPro;
//using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRow : MonoBehaviour
{
    public Image background;   // assign in inspector
    private static readonly Color equippedColor = new Color32(0xBC, 0xBC, 0xBC, 0xFF);
    private static readonly Color normalColor = Color.white;

    public TextMeshProUGUI useButtonText;



    [SerializeField] private Image image;
    [SerializeField] private Image durabilityImage;
    [SerializeField] private Image durabilityImageBackground;
    [SerializeField] private Image brokenImage;


    //[SerializeField] private TextMeshProUGUI idText;
    //[SerializeField] private TextMeshProUGUI nameText;
    //[SerializeField] private TextMeshProUGUI typeText;
    //[SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button useButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button dropButton;

    private ItemInstance linkedItem;         // reference to the actual item
    private InventoryWindow parentWindow;    // reference to the UI window

    public void SetData(ItemInstance instance, InventoryWindow window)
    {
        background.color = instance.isEquipped ? equippedColor : normalColor;


        linkedItem = instance;
        parentWindow = window;


        image.sprite = instance.itemSO.icon;

        if (!instance.itemSO.isBreakable)
        {
            durabilityImageBackground.gameObject.SetActive(false);
        }
        else
        {
            durabilityImage.gameObject.SetActive(true);

            // 1. Calculate fill amount (0 to 1)
            float fill = (float)(instance.currentDurability / instance.itemSO.maxDurability);
            durabilityImage.fillAmount = fill;

            // 2. Change color depending on fill
            if (fill <= 0.2f)
                durabilityImage.color = Color.red;     // <10% durability
            else
                durabilityImage.color = Color.green;   // >10% durability

            if (instance.currentDurability <= 0)
                brokenImage.gameObject.SetActive(true);
        }

        //idText.text = instance.itemSO.ID.ToString();
        //nameText.text = instance.itemSO.itemName;
        // typeText.text = instance.itemSO.category.ToString();
        //durabilityText.text = instance.currentDurability.ToString();
        amountText.text = "x"+instance.quantity.ToString();

        useButtonText.text = instance.isEquipped ? "Unequip" : "Use";


        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(OnItemInfoPressed);

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(OnUsePressed);

        moveButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(OnMovePressed);

        dropButton.onClick.RemoveAllListeners();
        dropButton.onClick.AddListener(OnDropPressed);
    }

    private void OnItemInfoPressed()
    {
        parentWindow.OnInfoButtonPressed(linkedItem);
    }

    private void OnUsePressed()
    {
        parentWindow.OnUseButtonPressed(linkedItem);
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
