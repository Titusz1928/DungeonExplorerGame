using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableDropdownUI : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [Header("UI References")]
    [SerializeField] private GameObject listContainer;
    [SerializeField] private Transform listParent;
    [SerializeField] private RectTransform sectionRoot;
    [SerializeField] private GameObject rowPrefab;

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
        if (isExpanded) RefreshConsumables();
        else CloseSection();
    }

    public void RefreshConsumables()
    {
        if (player == null) player = PlayerStateManager.Instance.gameObject;

        ClearList();

        Inventory inventory = player.GetComponent<Inventory>();
        List<ItemInstance> consumables = inventory.items.FindAll(i =>
            i.itemSO is ConsumableItemSO c &&
            (c.consumableType == ConsumableType.Food ||
             c.consumableType == ConsumableType.Potion ||
             c.consumableType == ConsumableType.FirstAid));

        if (consumables.Count == 0)
        {
            CloseSection();
            return;
        }

        listContainer.SetActive(true);
        expandCollapseIcon.sprite = collapseIcon;

        foreach (var item in consumables)
        {
            GameObject entry = Instantiate(rowPrefab, listParent);
            entry.GetComponent<ConsumableRowPrefab>().SetData(item);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listParent.GetComponent<RectTransform>());

        float listHeight = LayoutUtility.GetPreferredHeight(listParent.GetComponent<RectTransform>());
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HeaderHeight + listHeight);

        RebuildParentLayout();
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

    private void ClearList()
    {
        for (int i = listParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(listParent.GetChild(i).gameObject);
    }
}