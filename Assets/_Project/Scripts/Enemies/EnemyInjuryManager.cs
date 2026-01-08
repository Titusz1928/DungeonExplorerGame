using System.Collections.Generic;
using UnityEngine;

public class EnemyInjuryManager : MonoBehaviour
{
    public List<Injury> activeInjuries = new List<Injury>();
    private EnemyStats stats;

    [Header("Settings")]
    public float bandageMaxDuration = 120f;

    private void Start()
    {
        stats = GetComponent<EnemyStats>();
    }

    private void Update()
    {
        if (UIManager.Instance != null && !UIManager.Instance.IsInBattle)
        {
            ProcessInjuries(Time.deltaTime);
        }
    }

    public void OnTurnEnded()
    {
        // Use the central UI state like the player does
        if (UIManager.Instance != null && UIManager.Instance.IsInBattle)
        {
            ProcessInjuries(10f);
        }
    }

    private void ProcessInjuries(float deltaTime)
    {
        float totalDrain = 0;

        for (int i = activeInjuries.Count - 1; i >= 0; i--)
        {
            Injury injury = activeInjuries[i];
            totalDrain += injury.GetDamageAmount() * deltaTime;

            // Healing & Bandage logic (Same as player)
            injury.severity -= injury.healingRate * deltaTime;

            if (injury.severity <= 0)
            {
                activeInjuries.RemoveAt(i);
            }
        }

        if (totalDrain > 0)
        {
            ApplyBleedDamage(totalDrain);
        }
    }

    private void ApplyBleedDamage(double damage)
    {
        stats.TakeDamage(damage);
    }

    public void AddInjury(ArmorSlot slot, InjuryType type, float severity)
    {
        float multiplier = 1.0f;

        // Look up the specific body part in the EnemySO data to get the multiplier
        if (stats != null && stats.GetController().data != null)
        {
            var partData = stats.GetController().data.anatomy.Find(p => p.associatedSlot == slot);
            if (partData != null)
            {
                multiplier = partData.bleedMultiplier;
            }
        }

        // Pass the multiplier into the new Injury
        activeInjuries.Add(new Injury(slot, type, severity, multiplier));
        Debug.Log($"{name} suffered a {type} to the {slot} (Bleed Mod: {multiplier}x)!");
    }
}