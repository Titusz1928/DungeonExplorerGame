using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookController : MonoBehaviour
{
    [Header("Left Page Layers")]
    public GameObject leftImageLayer;    // bookpageimageL
    public GameObject leftTextLayer;     // bookpagetextL
    public Image leftImageComponent;
    public TextMeshProUGUI leftTextComponent;

    [Header("Right Page Layers")]
    public GameObject rightImageLayer;   // bookpageimageR
    public GameObject rightTextLayer;    // bookpagetextR
    public Image rightImageComponent;
    public TextMeshProUGUI rightTextComponent;

    public void Refresh(PageSideSO leftData, PageSideInstance leftInst, string leftSideName,
                        PageSideSO rightData, PageSideInstance rightInst, string rightSideName,
                        Sprite emptySprite)
    {
        // Refresh Left Side
        UpdateSide(leftData, leftInst, leftImageLayer, leftImageComponent, leftTextLayer, leftTextComponent, emptySprite);

        // Refresh Right Side
        UpdateSide(rightData, rightInst, rightImageLayer, rightImageComponent, rightTextLayer, rightTextComponent, emptySprite);
    }

    private void UpdateSide(PageSideSO data, PageSideInstance inst,
                            GameObject imgObj, Image imgComp,
                            GameObject txtObj, TextMeshProUGUI txtComp,
                            Sprite empty)
    {
        // Default everything to off first to ensure a clean state
        if (imgObj != null) imgObj.SetActive(false);
        if (txtObj != null) txtObj.SetActive(false);

        // Case 1: No Data (Show Empty Page Background)
        if (data == null)
        {
            if (imgObj != null) imgObj.SetActive(true);
            if (imgComp != null) imgComp.sprite = empty;
            return;
        }

        // Case 2: Data has a Background Sprite (Image Page)
        if (data.background != null)
        {
            if (imgObj != null) imgObj.SetActive(true);
            if (imgComp != null) imgComp.sprite = data.background;
        }
        // Case 3: Data has no Sprite (Text Page)
        else
        {
            if (txtObj != null) txtObj.SetActive(true);
            if (txtComp != null)
            {
                txtComp.text = (inst != null && !string.IsNullOrEmpty(inst.customText))
                               ? inst.customText
                               : data.defaultText;
            }
        }
    }
}