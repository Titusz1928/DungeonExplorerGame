using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEnemyUISlot : MonoBehaviour
{
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
}