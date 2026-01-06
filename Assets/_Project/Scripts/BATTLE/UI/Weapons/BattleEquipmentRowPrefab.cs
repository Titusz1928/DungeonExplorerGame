using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEquipmentRowPrefab : MonoBehaviour, IItemRow
{
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image durabilityBar;
    [SerializeField] private GameObject durabilityRoot;
    [SerializeField] private GameObject brokenIcon;
    [SerializeField] private TextMeshProUGUI useButtonText;
    [SerializeField] private Button actionButton;

    private ItemInstance linkedItem;
    private static readonly Color equippedColor = new Color32(0xBC, 0xBC, 0xBC, 0xFF);
    private static readonly Color normalColor = Color.white;

    public void SetData(ItemInstance instance)
    {
        linkedItem = instance;

        // 1. Basic Info & Background
        itemIcon.sprite = instance.itemSO.icon;
        background.color = instance.isEquipped ? equippedColor : normalColor;
        useButtonText.text = instance.isEquipped ? "Unequip" : "Equip";

        // 2. Durability Logic
        if (instance.itemSO.isBreakable)
        {
            durabilityRoot.SetActive(true);
            float fill = (float)((float)instance.currentDurability / instance.itemSO.maxDurability);
            durabilityBar.fillAmount = fill;

            // Visual threshold feedback
            durabilityBar.color = (fill <= 0.2f) ? Color.red : Color.green;

            if (brokenIcon != null)
                brokenIcon.SetActive(instance.currentDurability <= 0);
        }
        else
        {
            durabilityRoot.SetActive(false);
            if (brokenIcon != null) brokenIcon.SetActive(false);
        }

        // 3. Button Setup
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnActionPressed);
    }

    private void OnActionPressed()
    {
        // 1. Logic first
        if (linkedItem.isEquipped)
            EquipmentManager.Instance.Unequip(linkedItem);
        else
            EquipmentManager.Instance.Equip(linkedItem);

        // 2. Refresh UI while it's still visible/active
        RefreshAllDropdowns();

        // 3. Then end the turn (which deactivates the panel)
        if (UIManager.Instance.IsInBattle)
        {
            BattleManager.Instance.SetPendingAction(PlayerActionType.EquipmentChange);
            BattleManager.Instance.OnEndTurnPressed();
        }
    }

    private void RefreshAllDropdowns()
    {
        // Find all Category UIs
        CategoryDropdownUI[] categoryUIs = Object.FindObjectsByType<CategoryDropdownUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ui in categoryUIs)
        {
            // Even if it's currently hidden in a tab, refresh it 
            // so it's correct when the player clicks the tab again
            ui.Refresh();
        }

        // Refresh Labels
        EquipmentLabelUI labelUI = Object.FindFirstObjectByType<EquipmentLabelUI>();
        if (labelUI != null) labelUI.RefreshLabels();
    }
}