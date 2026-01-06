using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum BattleTab { Log, Player }

public class BattleUIManager : MonoBehaviour
{

    public static BattleUIManager Instance;
    [SerializeField] private InjuryDatabase injuryDatabase;

    [Header("Windows")]
    [SerializeField] public GameObject treatInjuryWindow;

    [Header("Tab Panels")]
    [SerializeField] private GameObject logPanel;
    [SerializeField] private GameObject playerPanel;

    [Header("Slots")]
    [SerializeField] private List<BattleEnemyUISlot> uiSlots;

    [Header("Skip Log UI")]
    [SerializeField] private Image skipLogButtonBg;

    private void Awake() => Instance = this;

    private void Start()
    {
        UpdateSkipLogVisuals();
    }

    public void SwitchToLogTab() => ShowTab(BattleTab.Log);
    public void SwitchToPlayerTab() => ShowTab(BattleTab.Player);

    private void ShowTab(BattleTab tab)
    {
        // Disable all first
        logPanel.SetActive(false);
        playerPanel.SetActive(false);

        switch (tab)
        {
            case BattleTab.Log:
                logPanel.SetActive(true);
                break;
            case BattleTab.Player:
                playerPanel.SetActive(true);
                break;
        }
    }

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

    public void ToggleSkipLog()
    {
        // Toggle the logic state in the Manager
        BattleManager.Instance.isSkipLogEnabled = !BattleManager.Instance.isSkipLogEnabled;

        // Update the visual transparency
        UpdateSkipLogVisuals();
    }

    private void UpdateSkipLogVisuals()
    {
        if (skipLogButtonBg == null) return;

        Color c = skipLogButtonBg.color;
        // If enabled: Full alpha (1). If disabled: Transparent (0).
        c.a = BattleManager.Instance.isSkipLogEnabled ? 1f : 0f;
        skipLogButtonBg.color = c;
    }
}