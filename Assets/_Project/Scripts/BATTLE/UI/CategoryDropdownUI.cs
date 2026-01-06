using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DropdownType { Consumables, Equipment, Armor }

public class CategoryDropdownUI : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [Header("Configuration")]
    [SerializeField] private DropdownType type;
    [SerializeField] private GameObject rowPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject listContainer;
    [SerializeField] private Transform listParent;
    [SerializeField] private RectTransform sectionRoot;

    [Header("Icons")]
    [SerializeField] private Image expandCollapseIcon;
    [SerializeField] private Sprite expandIcon;
    [SerializeField] private Sprite collapseIcon;

    [Header("Settings")]
    [SerializeField] private float HeaderHeight = 100f;

    private bool isExpanded = false;

    public void Toggle()
    {
        isExpanded = !isExpanded;
        // If we just turned it on, refresh the data and show it
        if (isExpanded)
        {
            Refresh();
        }
        else
        {
            CloseSection();
        }
    }

    public void Refresh()
    {
        // SAFETY CHECK: If the Inspector fields are empty, skip this object
        if (listParent == null || listContainer == null || sectionRoot == null)
        {
            Debug.LogWarning($"[CategoryDropdownUI] skipping refresh on {gameObject.name} because references are not assigned.");
            return;
        }

        // NEW LOGIC: If the dropdown is closed, don't force it open!
        if (!isExpanded)
        {
            CloseSection(); // This ensures visuals are shut and height is reset
            return;
        }

        if (player == null) player = PlayerStateManager.Instance.gameObject;

        // 1. Clear existing rows
        ClearList();

        Inventory inventory = player.GetComponent<Inventory>();
        List<ItemInstance> itemsToDisplay = GetFilteredItems(inventory);

        // 2. If the last item was used/removed, close the section
        if (itemsToDisplay == null || itemsToDisplay.Count == 0)
        {
            CloseSection();
            return;
        }

        // 3. Populate (Only happens if isExpanded is true)
        listContainer.SetActive(true);
        expandCollapseIcon.sprite = collapseIcon;

        foreach (var item in itemsToDisplay)
        {
            GameObject entry = Instantiate(rowPrefab, listParent);
            IItemRow rowScript = entry.GetComponent<IItemRow>();
            if (rowScript != null)
            {
                rowScript.SetData(item);
            }
        }

        // 4. Recalculate Layout
        StartCoroutine(ForceUIRefresh());
    }

    private void ClearList()
    {
        // Extra safety check here too
        if (listParent == null) return;

        for (int i = listParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(listParent.GetChild(i).gameObject);
        }
    }

    private IEnumerator ForceUIRefresh()
    {
        // Wait for the end of the frame so Instantiate is fully finished
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        if (listParent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(listParent.GetComponent<RectTransform>());

        float listHeight = LayoutUtility.GetPreferredHeight(listParent.GetComponent<RectTransform>());
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HeaderHeight + listHeight);

        RebuildParentLayout();
    }

    private List<ItemInstance> GetFilteredItems(Inventory inv)
    {
        switch (type)
        {
            case DropdownType.Consumables:
                return inv.items.FindAll(i => i.itemSO is ConsumableItemSO);

            case DropdownType.Equipment:
                // This covers Weapons and Shields
                return inv.items.FindAll(i => i.itemSO is WeaponItemSO || i.itemSO is ShieldItemSO);

            case DropdownType.Armor:
                // This covers all Armor pieces (Head, Chest, etc.)
                return inv.items.FindAll(i => i.itemSO is ArmorItemSO);

            default:
                return new List<ItemInstance>();
        }
    }

    private void CloseSection()
    {
        isExpanded = false;
        listContainer.SetActive(false);
        expandCollapseIcon.sprite = expandIcon;
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HeaderHeight);
        RebuildParentLayout();
    }

    private void RebuildParentLayout()
    {
        if (transform.parent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

}