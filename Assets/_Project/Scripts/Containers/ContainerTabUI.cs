using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContainerTab : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    //[SerializeField] private TMP_Text labelText;
    [SerializeField] private Button button;

    [SerializeField] private Sprite fallbackIcon;

    private WorldContainer container;
    private PickupWindow window;
    private bool isGroundTab = false;



    // Special setup for the Ground tab
    public void SetGroundTab(PickupWindow pw, Sprite icon)
    {
        isGroundTab = true;
        window = pw;

        //labelText.text = "Ground";
        iconImage.sprite = icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            window.ShowGroundItems();
        });
    }

    // Regular container tab
    public void SetContainerData(WorldContainer wc, PickupWindow pw)
    {
        container = wc;
        window = pw;

        //labelText.text = wc.containerData.containerName;
        // Safety Check: Use a fallback if containerData is missing
        if (wc.containerData != null)
        {
            // labelText.text = wc.containerData.containerName;
            iconImage.sprite = wc.containerData.containerIcon;
        }
        else
        {
            Debug.LogWarning($"Container UI for {wc.name} is missing ContainerSO!");
            iconImage.sprite = fallbackIcon; // Optional: set a default "Bag" icon
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            window.ShowContainerItems(container);
        });
    }
}
