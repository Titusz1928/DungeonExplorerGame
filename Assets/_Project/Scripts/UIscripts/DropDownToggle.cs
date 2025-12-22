using UnityEngine;
using UnityEngine.UI;

public class DropDownToggle : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isExpanded = false;

    [Header("Sections")]
    [SerializeField] private RectTransform totalDropdownArea;
    [SerializeField] private RectTransform topSection;
    [SerializeField] private GameObject bottomSection;

    [Header("Icon")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite expandIcon;
    [SerializeField] private Sprite collapseIcon;

    [Header("Heights")]
    [SerializeField] private float collapsedHeight = 200f;
    [SerializeField] private float expandedHeight = 1000f;

    private void Awake()
    {
        // Ensure correct initial state in editor & play mode
        ApplyState();
    }

    public void Toggle()
    {
        isExpanded = !isExpanded;
        ApplyState();
    }

    private void ApplyState()
    {
        SetSectionHeight();
        SetTopSectionAnchors();
        SetBottomSection();
        SetIcon();
    }

    private void SetSectionHeight()
    {
        if (totalDropdownArea == null) return;

        Vector2 size = totalDropdownArea.sizeDelta;
        size.y = isExpanded ? expandedHeight : collapsedHeight;
        totalDropdownArea.sizeDelta = size;
    }

    private void SetTopSectionAnchors()
    {
        if (topSection == null) return;

        if (isExpanded)
        {
            topSection.anchorMin = new Vector2(0f, 0.8f);
            topSection.anchorMax = new Vector2(1f, 1f);
        }
        else
        {
            // Fully stretched
            topSection.anchorMin = Vector2.zero;
            topSection.anchorMax = Vector2.one;
        }

        // Reset offsets so anchors fully control layout
        topSection.offsetMin = Vector2.zero;
        topSection.offsetMax = Vector2.zero;
    }

    private void SetBottomSection()
    {
        if (bottomSection != null)
            bottomSection.SetActive(isExpanded);
    }

    private void SetIcon()
    {
        if (iconImage == null) return;

        iconImage.sprite = isExpanded ? collapseIcon : expandIcon;
    }
}
