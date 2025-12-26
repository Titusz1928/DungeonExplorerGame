using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InjuryDropdownUI : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [Header("UI References")]
    [SerializeField] private GameObject injuryListContainer;
    [SerializeField] private Transform injuryListParent;
    [SerializeField] private RectTransform sectionRoot;
    [SerializeField] private GameObject injuryRowPrefab;

    [Header("Icons")]
    [SerializeField] private Image expandCollapseIcon;
    [SerializeField] private Sprite expandIcon;
    [SerializeField] private Sprite collapseIcon;

    [Header("Settings")]
    [SerializeField] private float headerHeight = 200f;

    private bool isExpanded = false;

    // Call this from the InventoryWindow right after instantiating
    public void Initialize(GameObject playerRef)
    {
        player = playerRef;
        // Optional: Auto-refresh if it was already expanded
        if (isExpanded) RefreshInjuries();
    }

    private void OnEnable()
    {
        if (isExpanded) RefreshInjuries();
    }

    public void Toggle()
    {
        isExpanded = !isExpanded;
        if (isExpanded) RefreshInjuries();
        else CloseSection();
    }

    private void RefreshInjuries()
    {
        // If player is null, try to find the Instance dynamically
        if (player == null)
        {
            if (PlayerStateManager.Instance != null)
            {
                player = PlayerStateManager.Instance.gameObject;
            }
            else
            {
                Debug.LogError("[InjuryUI] Refresh failed: PlayerStateManager Instance not found in scene!");
                return;
            }
        }

        ClearList();

        var injuries = player.GetComponent<InjuryManager>().activeInjuries;

        if (injuries == null || injuries.Count == 0)
        {
            CloseSection();
            return;
        }

        injuryListContainer.SetActive(true);
        expandCollapseIcon.sprite = collapseIcon;

        foreach (var injury in injuries)
        {
            GameObject entry = Instantiate(injuryRowPrefab, injuryListParent);
            entry.GetComponent<InjuryRowPrefab>().SetData(injury);
        }

        // Wait a frame or force canvas update so UI elements calculate their sizes
        Canvas.ForceUpdateCanvases();

        // Rebuild the list layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(injuryListParent.GetComponent<RectTransform>());

        float listHeight = LayoutUtility.GetPreferredHeight(injuryListParent.GetComponent<RectTransform>());
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, headerHeight + listHeight);

        // Tell the Skill Row Container to adjust its layout
        RebuildParentLayout();
    }

    private void CloseSection()
    {
        isExpanded = false;
        injuryListContainer.SetActive(false);
        expandCollapseIcon.sprite = expandIcon;
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, headerHeight);

        RebuildParentLayout();
    }

    private void RebuildParentLayout()
    {
        if (transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
    }

    private void ClearList()
    {
        for (int i = injuryListParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(injuryListParent.GetChild(i).gameObject);
        }
    }
}