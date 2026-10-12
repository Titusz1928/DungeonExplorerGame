using System.Collections.Generic;
using System.Linq;
using TitusGames.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleTab { Log, Player }

public class BattleUIManager : MonoBehaviour, IBattleUIService
{
    [SerializeField] private InjuryDatabase injuryDatabase;

    [Header("UI Controls")]
    [SerializeField] private TMP_Text logText;
    [SerializeField] private Button endTurnButton;

    [Header("Windows")]
    [SerializeField] private GameObject _treatInjuryWindow;
    public GameObject treatInjuryWindow => _treatInjuryWindow;

    [Header("Tab Panels")]
    [SerializeField] private GameObject logPanel;
    [SerializeField] private GameObject playerPanel;

    [Header("Slots")]
    [SerializeField] private List<BattleEnemyUISlot> uiSlots;

    [Header("Skip Log UI")]
    [SerializeField] private Image skipLogButtonBg;

    [Header("Anatomy Settings")]
    [SerializeField] private GameObject partButtonPrefab;

    private void OnEnable()
    {
        ServiceLocator.Current.Register<IBattleUIService>(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Current.Unregister<IBattleUIService>();
    }

    private void Start()
    {
        UpdateSkipLogVisuals();
    }

    public void SetLogText(string text)
    {
        if (logText != null)
        {
            logText.text = text;
        }
    }

    public void SetEndTurnButtonInteractable(bool interactable)
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable;
        }
    }

    public void SetupEnemySlots(List<EnemyController> activeCombatants, int targetedIndex, System.Action<int> onEnemyClicked)
    {
        Debug.Log("[BattleUIManager] Setting up enemy slots...");

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] == null) continue;

            if (i < activeCombatants.Count && activeCombatants[i] != null)
            {
                // 1. Activate the HP bar object (where BattleEnemyUISlot lives)
                uiSlots[i].gameObject.SetActive(true);

                // 2. Activate and assign the separate portrait image
                if (uiSlots[i].enemyPortrait != null)
                {
                    Sprite sprite = activeCombatants[i].data.battlesprite;
                    uiSlots[i].enemyPortrait.gameObject.SetActive(sprite != null);
                    uiSlots[i].enemyPortrait.sprite = sprite;
                }

                // 3. Wire button click
                int index = i;
                Button btn = uiSlots[i].GetComponent<Button>();
                if (btn == null) btn = uiSlots[i].GetComponentInParent<Button>();

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => onEnemyClicked?.Invoke(index));
                }
            }
            else
            {
                // Deactivate both the HP bar object AND the separate portrait image
                uiSlots[i].gameObject.SetActive(false);

                if (uiSlots[i].enemyPortrait != null)
                {
                    uiSlots[i].enemyPortrait.gameObject.SetActive(false); // Fixed: added .gameObject
                }

                if (uiSlots[i].anatomyOverlayRoot != null)
                {
                    uiSlots[i].anatomyOverlayRoot.SetActive(false);
                }
            }
        }

        UpdateTargetVisuals(targetedIndex, string.Empty);
    }

    public void UpdateTargetVisuals(int targetIndex, string targetPartName)
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            bool isTargeted = (i == targetIndex);

            // If your BattleEnemyUISlot component contains visual target state logic
            if (uiSlots[i] != null)
            {
                uiSlots[i].SetTargetVisuals(isTargeted, targetPartName);
            }
        }
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
        var battleService = ServiceLocator.Current?.Get<IBattleService>();
        var skillService = ServiceLocator.Current?.Get<IPlayerSkillService>();

        if (battleService == null || skillService == null) return;

        var combatants = battleService.GetActiveCombatants();
        int playerIQ = skillService.GetLevel(PlayerSkill.BattleIQ);

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

        skillService.AddXP(PlayerSkill.BattleIQ, 1, true);
    }

    private void UpdateSlot(BattleEnemyUISlot slot, EnemyController enemy, int iqLevel)
    {
        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        // 1. Calculate Variance (e.g., 40% at lvl 0, 2% at lvl 100)
        float decayRate = 0.15f; // Adjust this to make it faster/slower
        double varianceMultiplier = 0.02f + (0.38f * Mathf.Exp(-decayRate * iqLevel));
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
        var battleService = ServiceLocator.Current?.Get<IBattleService>();

        if (battleService != null)
        {
            battleService.IsSkipLogEnabled = !battleService.IsSkipLogEnabled;
        }

        // Update the visual transparency
        UpdateSkipLogVisuals();
    }

    private void UpdateSkipLogVisuals()
    {
        if (skipLogButtonBg == null) return;

        var battleService = ServiceLocator.Current?.Get<IBattleService>();
        bool isSkipEnabled = battleService != null && battleService.IsSkipLogEnabled;

        Color c = skipLogButtonBg.color;
        c.a = isSkipEnabled ? 1f : 0f;
        skipLogButtonBg.color = c;
    }

    public void ToggleAnatomyOverlay()
    {
        var battleService = ServiceLocator.Current?.Get<IBattleService>();
        if (battleService == null) return;

        int targetIndex = battleService.TargetedEnemyIndex;
        var combatants = battleService.GetActiveCombatants();

        if (targetIndex < 0 || targetIndex >= combatants.Count) return;

        BattleEnemyUISlot slotUI = uiSlots[targetIndex];


        Button mainSlotButton = slotUI.GetComponent<Button>();
        if (mainSlotButton == null) mainSlotButton = slotUI.GetComponentInParent<Button>();

        // 1. If it's already active, hide it and stop
        if (slotUI.anatomyOverlayRoot.activeSelf)
        {
            HideAnatomyOverlay();
            if (mainSlotButton != null) mainSlotButton.interactable = true;
            return;
        }

        // 2. Otherwise, hide all others first (clean slate)
        HideAnatomyOverlay();

        if (mainSlotButton != null)
        {
            mainSlotButton.enabled = false;
            if (mainSlotButton.targetGraphic != null)
                mainSlotButton.targetGraphic.raycastTarget = false; // MOUSE PASSES THROUGH NOW
        }

        // 3. Show this specific one
        EnemyController currentEnemy = combatants[targetIndex];
        slotUI.anatomyOverlayRoot.SetActive(true);

        // 4. Clear and rebuild buttons
        foreach (Transform child in slotUI.anatomyButtonContainer) Destroy(child.gameObject);

        foreach (var part in currentEnemy.data.anatomy)
        {
            if (part.partClickSprite == null) continue;

            GameObject btnObj = Instantiate(partButtonPrefab, slotUI.anatomyButtonContainer);
            btnObj.GetComponent<BodyPartTargetButton>().Setup(part);
        }
    }

    public void ShowAnatomyOverlay()
    {
        var battleService = ServiceLocator.Current?.Get<IBattleService>();
        if (battleService == null) return;

        int targetIndex = battleService.TargetedEnemyIndex;
        var combatants = battleService.GetActiveCombatants();

        if (targetIndex < 0 || targetIndex >= combatants.Count) return;

        EnemyController currentEnemy = combatants[targetIndex];
        BattleEnemyUISlot slotUI = uiSlots[targetIndex];

        // 1. Show the root panel
        slotUI.anatomyOverlayRoot.SetActive(true);

        // 2. Clear old buttons
        foreach (Transform child in slotUI.anatomyButtonContainer) Destroy(child.gameObject);

        // 3. Spawn new custom-shaped buttons from EnemySO
        foreach (var part in currentEnemy.data.anatomy)
        {
            if (part.partClickSprite == null) continue;

            GameObject btnObj = Instantiate(partButtonPrefab, slotUI.anatomyButtonContainer);
            btnObj.GetComponent<BodyPartTargetButton>().Setup(part);
        }
    }

    public void HideAnatomyOverlay()
    {
        foreach (var slot in uiSlots)
        {
            if (slot.anatomyOverlayRoot != null)
            {
                slot.anatomyOverlayRoot.SetActive(false);

                Button mainSlotButton = slot.GetComponent<Button>();
                if (mainSlotButton == null) mainSlotButton = slot.GetComponentInParent<Button>();

                if (mainSlotButton != null)
                {
                    mainSlotButton.enabled = true;
                    // 2. Re-enable raycast so we can click the enemy again normally
                    if (mainSlotButton.targetGraphic != null)
                        mainSlotButton.targetGraphic.raycastTarget = true;
                }
            }
        }
    }
}