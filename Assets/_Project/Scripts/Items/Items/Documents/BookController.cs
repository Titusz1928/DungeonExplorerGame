using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookController : MonoBehaviour
{
    [Header("Left Page Components")]
    public Image leftImage;
    //public TextMeshProUGUI leftLabel;
    //public TextMeshProUGUI leftText; // Optional: if you want text support in books

    [Header("Right Page Components")]
    public Image rightImage;
    //public TextMeshProUGUI rightLabel;
    //public TextMeshProUGUI rightText;

    public void Refresh(PageSideSO leftData, PageSideInstance leftInst, string leftSideName,
                    PageSideSO rightData, PageSideInstance rightInst, string rightSideName,
                    Sprite emptySprite)
    {
        // Left Side
        if (leftData == null)
        {
            if (leftImage != null) leftImage.sprite = emptySprite;
        }
        else
        {
            if (leftImage != null) leftImage.sprite = leftData.background;
        }

        // Right Side
        if (rightData == null)
        {
            // Turn past last page: show empty sprite on right
            if (rightImage != null) rightImage.sprite = emptySprite;
        }
        else
        {
            if (rightImage != null) rightImage.sprite = rightData.background;
        }
    }
}