using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private float explorationHeaderHeight = 200f;
    [SerializeField] private float battleHeaderHeight = 100f;

    // Helper property to get the correct height based on the mode
    private float CurrentHeaderHeight => isBattleUI ? battleHeaderHeight : explorationHeaderHeight;

    [Header("Battle Settings")]
    [SerializeField] private bool isBattleUI = false;
    [SerializeField] private GameObject battleInjuryRowPrefab; // The more compact version

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

    public void RefreshInjuries()
    {
        // SAFETY: If it's closed, just ensure visuals match and bail.
        if (!isExpanded)
        {
            CloseSection();
            return;
        }

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

        GameObject prefabToUse = (isBattleUI && battleInjuryRowPrefab != null)
                             ? battleInjuryRowPrefab
                             : injuryRowPrefab;

        foreach (var injury in injuries)
        {
            GameObject entry = Instantiate(prefabToUse, injuryListParent);

            // Try to get the standard component
            var standardRow = entry.GetComponent<InjuryRowPrefab>();
            if (standardRow != null)
            {
                standardRow.SetData(injury);
            }
            else
            {
                // If that fails, try to get the battle component
                var battleRow = entry.GetComponent<BattlePlayerInjuryRowPrefab>();
                if (battleRow != null)
                {
                    battleRow.SetData(injury);
                }
                else
                {
                    Debug.LogError($"[InjuryUI] Prefab {entry.name} is missing an injury row script!");
                }
            }
        }

        // Wait a frame or force canvas update so UI elements calculate their sizes
        Canvas.ForceUpdateCanvases();

        // Rebuild the list layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(injuryListParent.GetComponent<RectTransform>());

        float listHeight = LayoutUtility.GetPreferredHeight(injuryListParent.GetComponent<RectTransform>());
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CurrentHeaderHeight + listHeight);

        // Tell the Skill Row Container to adjust its layout
        RebuildParentLayout();
    }

    private void CloseSection()
    {
        isExpanded = false;
        injuryListContainer.SetActive(false);
        expandCollapseIcon.sprite = expandIcon;
        sectionRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CurrentHeaderHeight);

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