using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("UI References")]
    [SerializeField] private List<Image> enemySlots; // The Image objects

    private List<EnemyController> activeCombatants = new List<EnemyController>();
    private int targetedEnemyIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var slot in enemySlots) slot.gameObject.SetActive(false);
    }

    public void StartBattle(EnemyController mainTarget, List<EnemyController> helpers)
    {
        activeCombatants.Clear();
        activeCombatants.Add(mainTarget);
        activeCombatants.AddRange(helpers);

        // Reset targeting to the first enemy
        targetedEnemyIndex = 0;

        for (int i = 0; i < enemySlots.Count; i++)
        {
            if (i < activeCombatants.Count)
            {
                EnemyController enemy = activeCombatants[i];
                enemySlots[i].sprite = enemy.data.battlesprite;
                enemySlots[i].gameObject.SetActive(true);
                enemySlots[i].SetNativeSize();

                // Setup the click event for this specific slot
                int index = i; // Local copy for the listener
                Button btn = enemySlots[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnEnemyClicked(index));
                }
            }
            else
            {
                enemySlots[i].gameObject.SetActive(false);
            }
        }

        UpdateTargetVisuals();
        Debug.Log($"Battle started. Target: {activeCombatants[targetedEnemyIndex].data.enemyName}");
    }

    public void OnEnemyClicked(int index)
    {
        if (index >= activeCombatants.Count) return;

        targetedEnemyIndex = index;
        Debug.Log($"New Target Selected: {activeCombatants[targetedEnemyIndex].data.enemyName}");

        UpdateTargetVisuals();
    }

    private void UpdateTargetVisuals()
    {
        for (int i = 0; i < enemySlots.Count; i++)
        {
            // Find the child named "border"
            Transform border = enemySlots[i].transform.Find("Border");
            if (border != null)
            {
                // Only show border if this slot is the targeted index AND the slot is active
                border.gameObject.SetActive(i == targetedEnemyIndex && enemySlots[i].gameObject.activeSelf);
            }
        }
    }

    // Helper to get the currently selected enemy for the attack logic later
    public EnemyController GetTargetedEnemy()
    {
        return activeCombatants[targetedEnemyIndex];
    }
}