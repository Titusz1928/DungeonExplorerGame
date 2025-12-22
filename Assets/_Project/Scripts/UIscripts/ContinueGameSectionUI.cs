using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinueGameSectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject worldListContainer;
    [SerializeField] private Transform worldListParent;
    [SerializeField] private Transform continueGameSection;
    [SerializeField] private GameObject worldEntryPrefab;

    [Header("Icons")]
    [SerializeField] private Image expandCollapseIcon;
    [SerializeField] private Sprite expandIcon;
    [SerializeField] private Sprite collapseIcon;



    private bool isExpanded = false;

    void Start()
    {
        var worlds = SaveSystem.LoadAllWorldMeta();
        if (worlds.Count == 0)
        {
            // Optionally grey out the button or hide the expand icon
            expandCollapseIcon.color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void Toggle()
    {
        isExpanded = !isExpanded;

        // We don't set the container active yet; 
        // we let PopulateWorldList decide if there's actually anything to show.
        if (isExpanded)
        {
            PopulateWorldList();
        }
        else
        {
            worldListContainer.SetActive(false);
            expandCollapseIcon.sprite = expandIcon;

            // Reset height to default (200)
            RectTransform continueRT = continueGameSection.GetComponent<RectTransform>();
            continueRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200f);
        }
    }

    private void PopulateWorldList()
    {
        ClearList();

        List<WorldMeta> worlds = SaveSystem.LoadAllWorldMeta()
            .OrderByDescending(w => ParseDate(w.updatedAt))
            .ToList();

        // --- NEW LOGIC: Check if list is empty ---
        if (worlds == null || worlds.Count == 0)
        {
            isExpanded = false; // Revert state
            worldListContainer.SetActive(false);
            expandCollapseIcon.sprite = expandIcon;
            Debug.Log("No worlds found, staying collapsed.");
            return;
        }

        // If we have worlds, proceed with expansion
        worldListContainer.SetActive(true);
        expandCollapseIcon.sprite = collapseIcon;

        foreach (var meta in worlds)
        {
            GameObject entry = Instantiate(worldEntryPrefab, worldListParent);
            entry.GetComponent<WorldEntryUI>().Initialize(meta);
        }

        // Force the Layout Group to calculate new positions immediately
        RectTransform bottomRT = worldListParent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(bottomRT);

        // Resize parent ContinueGameSection
        RectTransform continueRT = continueGameSection.GetComponent<RectTransform>();

        // Top section height (header)
        RectTransform topRT = worldListContainer.transform.parent.GetChild(0).GetComponent<RectTransform>();
        float topHeight = topRT.rect.height;

        // Bottom section height
        float bottomHeight = LayoutUtility.GetPreferredHeight(bottomRT);

        continueRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, topHeight + bottomHeight);
    }


    private void ClearList()
    {
        // Use a loop that works backwards when destroying immediately
        for (int i = worldListParent.childCount - 1; i >= 0; i--)
        {
            // DestroyImmediate ensures the LayoutGroup ignores these objects 
            // right now, rather than waiting for the end of the frame.
            DestroyImmediate(worldListParent.GetChild(i).gameObject);
        }
    }

    private DateTime ParseDate(string iso)
    {
        if (DateTime.TryParse(iso, out var dt))
            return dt;

        return DateTime.MinValue;
    }
}
