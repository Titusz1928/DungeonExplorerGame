using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;
    [SerializeField] private InjuryDatabase injuryDatabase;

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

        RefreshInjuries(slot, enemy, iqLevel);
    }

    private void RefreshInjuries(BattleEnemyUISlot slot, EnemyController enemy, int iqLevel)
    {
        // 1. Clear existing rows
        foreach (Transform child in slot.injuryIconContainer) Destroy(child.gameObject);

        var injuryManager = enemy.GetComponent<EnemyInjuryManager>();
        if (injuryManager == null || injuryManager.activeInjuries.Count == 0) return;

        List<Injury> injuries = injuryManager.activeInjuries;
        Transform currentRow = null;

        for (int i = 0; i < injuries.Count; i++)
        {
            // 2. Create a new row every 4 items
            if (i % 4 == 0)
            {
                GameObject rowObj = Instantiate(slot.rowPrefab, slot.injuryIconContainer);
                currentRow = rowObj.transform;
            }

            // 3. Instantiate the cell
            GameObject cellObj = Instantiate(slot.cellPrefab, currentRow);
            cellObj.GetComponent<BattleInjuryCell>().Setup(injuries[i], injuryDatabase);
        }
    }
}