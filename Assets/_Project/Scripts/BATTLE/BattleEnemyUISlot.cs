using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleEnemyUISlot : MonoBehaviour
{
    public Image lowEstimateBar;  // Child 1: Solid Fill
    public Image highEstimateBar; // Child 2: Semi-transparent Fill
    public TMP_Text hpText;       // The 70-90 text
    public Transform injuryIconContainer; // Where we will spawn injury icons later
}