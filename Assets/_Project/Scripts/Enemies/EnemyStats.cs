using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    private EnemyController controller;

    [Header("Combat Stats")]
    public double maxHP;
    public double currentHP;
    public float strength; // derived from HP

    public void Initialize(EnemyController owner)
    {
        controller = owner;
        GenerateCombatStats();
    }

    private void GenerateCombatStats()
    {
        if (controller.data == null) return;

        // HP variance (90-110%)
        float hpRoll = Random.Range(0.9f, 1.1f);
        maxHP = Mathf.RoundToInt(controller.data.maxHealth * hpRoll);
        currentHP = maxHP;

        // Strength derived from the HP roll
        strength = (float)maxHP / controller.data.maxHealth;

        Debug.Log($"{name} Stats Generated | HP: {currentHP} | Strength: {strength:F2}");
    }

    public void TakeDamage(double damage)
    {
        currentHP -= damage;
        Debug.Log($"{name} took {damage} damage. HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            // The controller handles the physical death/corpse logic
            controller.Die();
        }
    }
}