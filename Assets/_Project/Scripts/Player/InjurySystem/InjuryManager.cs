using System.Collections.Generic;
using UnityEngine;

public class InjuryManager : MonoBehaviour
{
    public List<Injury> activeInjuries = new List<Injury>();
    private PlayerStateManager playerState;

    [Header("Settings")]
    public float bandageMaxDuration = 120f; // 2 minutes or 20 turns?

    private void Start()
    {
        playerState = GetComponent<PlayerStateManager>();
    }

    private void Update()
    {
        // 1. Use the UIManager's central state
        // Only run real-time logic if NOT in combat
        if (UIManager.Instance != null && !UIManager.Instance.IsInBattle)
        {
            ProcessInjuries(Time.deltaTime);
        }
    }

    // Call this from BattleManager at the end of ExecuteBattleTurn
    public void OnTurnEnded()
    {
        // 2. Extra safety check to ensure we only tick turn-damage during battle
        if (UIManager.Instance != null && UIManager.Instance.IsInBattle)
        {
            // Treat 1 turn as 10 seconds of "real-time" for balance
            ProcessInjuries(10f);
        }
    }

    private void ProcessInjuries(float deltaTime)
    {
        float totalDrain = 0;

        for (int i = activeInjuries.Count - 1; i >= 0; i--)
        {
            Injury injury = activeInjuries[i];

            // 1. Drain Health
            totalDrain += injury.GetDamageAmount() * deltaTime;

            // 2. Progress Healing
            injury.severity -= injury.healingRate * deltaTime;

            // 3. Bandage Degradation
            if (injury.isBandaged && !injury.bandageDirty)
            {
                injury.bandageLifetime -= deltaTime;
                if (injury.bandageLifetime <= 0)
                {
                    injury.bandageDirty = true;
                }
            }

            // 4. Remove healed injuries
            if (injury.severity <= 0)
            {
                activeInjuries.RemoveAt(i);
                Debug.Log($"Injury on {injury.bodyPart} has healed!");
            }
        }

        if (totalDrain > 0)
        {
            playerState.inflictDamage(totalDrain);
        }
    }

    public void AddInjury(ArmorSlot part, InjuryType type, float severity)
    {
        activeInjuries.Add(new Injury(part, type, severity));
    }

    public void RemoveInjury(Injury injury)
    {
        if (activeInjuries.Contains(injury))
        {
            activeInjuries.Remove(injury);
            Debug.Log($"Specific {injury.type} on {injury.bodyPart} removed.");
        }
    }

    public Injury GetFirstTreatableInjury()
    {
        // Find the first injury that is NOT bandaged OR has a dirty bandage
        // We prioritize injuries that are currently draining health
        return activeInjuries.Find(injury => !injury.isBandaged || injury.bandageDirty);
    }

    public void ApplyBandage(Injury injury)
    {
        // No Find() needed. 'injury' IS the specific instance from the list.
        injury.isBandaged = true;
        injury.bandageDirty = false;
        injury.bandageLifetime = bandageMaxDuration;
    }
}