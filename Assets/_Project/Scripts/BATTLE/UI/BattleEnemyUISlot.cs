using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEnemyUISlot : MonoBehaviour
{
    [Header("Portrait")]
    public Image enemyPortrait;

    [Header("Health & Stats")]
    public Image lowEstimateBar;
    public Image highEstimateBar;
    public TMP_Text hpText;

    [Header("Injury UI Setup")]
    public Transform injuryIconContainer; // The parent (Vertical Layout Group)
    public GameObject rowPrefab;          // Prefab with Horizontal Layout Group
    public GameObject cellPrefab;         // Prefab with the 2 images (BattleInjuryCell)

    [Header("Anatomy Overlay")]
    public GameObject anatomyOverlayRoot; // The parent panel that holds the buttons
    public Transform anatomyButtonContainer;

    [Header("Targeting Visuals")]
    public GameObject targetBorder;
    public TMP_Text targetPartText;
    public GameObject targetBodypartSelector;

    public void SetTargetVisuals(bool isTargeted, string targetPartName)
    {
        // 1. Update Border
        if (targetBorder != null)
        {
            targetBorder.SetActive(isTargeted);
        }

        // 2. Update Bodypart Selector Container
        if (targetBodypartSelector != null)
        {
            targetBodypartSelector.SetActive(isTargeted);
        }

        // 3. Update Text (converted to uppercase just like your old code)
        if (targetPartText != null)
        {
            bool showText = isTargeted && !string.IsNullOrEmpty(targetPartName);
            targetPartText.gameObject.SetActive(showText);

            if (showText)
            {
                targetPartText.text = targetPartName.ToUpper();
            }
            else if (isTargeted)
            {
                targetPartText.text = "???";
                Debug.LogWarning("BattleManager: currentTargetPartName is null or empty!");
            }
        }
    }
}