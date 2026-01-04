using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("Slots")]
    [SerializeField] private List<BattleEnemyUISlot> uiSlots;

    private void Awake() => Instance = this;

    public void RefreshAll()
    {
        var combatants = BattleManager.Instance.GetActiveCombatants();
        int playerIQ = PlayerSkillManager.Instance.GetLevel(PlayerSkill.BattleIQ);

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < combatants.Count && combatants[i] != null)
            {
                uiSlots[i].gameObject.SetActive(true);
                UpdateSlot(uiSlots[i], combatants[i], playerIQ);
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }

        PlayerSkillManager.Instance.AddXP(PlayerSkill.BattleIQ, 1);
    }

    private void UpdateSlot(BattleEnemyUISlot slot, EnemyController enemy, int iqLevel)
    {
        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        // 1. Calculate Variance (e.g., 40% at lvl 0, 2% at lvl 100)
        double varianceMultiplier = Mathf.Lerp(0.4f, 0.02f, iqLevel / 100f);
        double varianceAmount = stats.maxHP * varianceMultiplier;

        // 2. Define the Range
        double lowEst = stats.currentHP - varianceAmount;
        double highEst = stats.currentHP + varianceAmount;

        // 3. ENFORCE LIMITS:
        // If the enemy is alive, the low estimate should never show 0.
        // We also clamp highEst so it doesn't exceed MaxHP.
        if (stats.currentHP > 0)
        {
            lowEst = Mathf.Max(1f, (float)lowEst);
        }
        else
        {
            lowEst = 0f;
        }

        highEst = Mathf.Clamp((float)highEst, (float)lowEst, (float)stats.maxHP);

        // 4. Update Text
        slot.hpText.text = $"{Mathf.RoundToInt((float)lowEst)} - {Mathf.RoundToInt((float)highEst)}";

        // 5. Update Bars (Horizontal Fill)
        slot.lowEstimateBar.fillAmount = (float)(lowEst / stats.maxHP);
        slot.highEstimateBar.fillAmount = (float)(highEst / stats.maxHP);
    }
}