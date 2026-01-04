using System.Collections.Generic;
using UnityEngine;

public class EnemyInjuryManager : MonoBehaviour
{
    public List<Injury> activeInjuries = new List<Injury>();
    private EnemyStats stats;

    [Header("Settings")]
    public float bandageMaxDuration = 120f;
    public bool isInCombat = false;

    private void Start()
    {
        stats = GetComponent<EnemyStats>();
    }

    private void Update()
    {
        // Enemies also process injuries in real-time (e.g., bleeding while chasing)
        if (!isInCombat)
        {
            ProcessInjuries(Time.deltaTime);
        }
    }

    public void OnTurnEnded()
    {
        if (isInCombat)
        {
            // 1 turn = 10 seconds of bleeding
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

    public void AddInjury(ArmorSlot part, InjuryType type, float severity)
    {
        activeInjuries.Add(new Injury(part, type, severity));
        Debug.Log($"{name} suffered a {type} to the {part}!");
    }
}