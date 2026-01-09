using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SinglePageController : MonoBehaviour
{
    [Header("Image Version Components")]
    public Image pageImage;
    public RawImage inkOverlay; // For future ink layers

    [Header("Text Version Components")]
    public TextMeshProUGUI textDisplay;

    [Header("UI Labels")]
    public TextMeshProUGUI sideLabel;

    // Updates the visual/text based on the side provided
    public void Refresh(PageSideSO data, PageSideInstance instance, bool isImageBased, string sideName)
    {
        if (sideLabel != null) sideLabel.text = sideName;

        // We still toggle children just in case the prefab contains both
        if (pageImage != null) pageImage.gameObject.SetActive(isImageBased);

        // Note: Use a null check on the parent if your Text is nested in a ScrollView
        if (textDisplay != null && textDisplay.transform.parent != null)
        {
            // This assumes Text -> Content -> Viewport -> ScrollView hierarchy
            GameObject scrollRoot = textDisplay.transform.parent.parent.gameObject;
            scrollRoot.SetActive(!isImageBased);
        }

        if (isImageBased && pageImage != null)
        {
            pageImage.sprite = data.background;
        }
        else if (!isImageBased && textDisplay != null)
        {
            textDisplay.text = !string.IsNullOrEmpty(instance.customText) ? instance.customText : data.defaultText;
        }
    }
}