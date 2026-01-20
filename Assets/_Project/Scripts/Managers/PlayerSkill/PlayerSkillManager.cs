using UnityEngine;
using System.Collections.Generic;

public enum PlayerSkill
{
    Speed,
    Strength,
    Stealth,
    BattleIQ,
    IQ,
    Charisma,
    WeaponHandling,
    Archery
}

[System.Serializable]
public class SkillData
{
    public int level = 1;
    public float currentXP = 0;
    public float xpToNextLevel = 100;

    public void RecalculateXPRequirement()
    {
        xpToNextLevel = 100 * Mathf.Pow(1.25f, level);
    }
}

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager Instance;

    private Dictionary<PlayerSkill, SkillData> skills =
        new Dictionary<PlayerSkill, SkillData>();

    private void Awake()
    {
        Instance = this;

        foreach (PlayerSkill skill in System.Enum.GetValues(typeof(PlayerSkill)))
        {
            skills[skill] = new SkillData();
        }
    }

    // ---------------------------------------------------------
    // IQ Modifier
    // ---------------------------------------------------------
    private float GetIQXPModifier()
    {
        SkillData iq = skills[PlayerSkill.IQ];

        // Remap IQ level 1–10 to XP multiplier 0.5 → 2.0
        return Mathf.Lerp(0.5f, 2f, iq.level / 10f);
    }

    // ---------------------------------------------------------
    // XP Adding
    // ---------------------------------------------------------
    public void AddXP(PlayerSkill skill, float baseAmount, bool notifyPlayer)
    {
        float modifier = GetIQXPModifier();
        float amount = baseAmount * modifier;

        SkillData data = skills[skill];
        data.currentXP += amount;

        while (data.currentXP >= data.xpToNextLevel)
        {
            data.currentXP -= data.xpToNextLevel;
            data.level++;
            data.RecalculateXPRequirement();

            Debug.Log($"{skill} leveled up to Level {data.level}!");

            if (notifyPlayer)
            {
                MessageManager.Instance.ShowMessageDirectly($"{skill} increased to {data.level}!");
                AudioManager.Instance.PlayLevelUpSFX();
            }           


        }
    }

    // ---------------------------------------------------------
    // Access helpers
    // ---------------------------------------------------------
    public int GetLevel(PlayerSkill skill) => skills[skill].level;
    public float GetXP(PlayerSkill skill) => skills[skill].currentXP;
    public float GetXPToNext(PlayerSkill skill) => skills[skill].xpToNextLevel;

    public SkillData GetSkillData(PlayerSkill skill)
    {
        return skills[skill];
    }

    // ---------------------------------------------------------
    // Debug Console Skill Name Parser
    // ---------------------------------------------------------
    public bool TryAddXP(string skillName, float amount)
    {
        foreach (var pair in skills)
        {
            if (pair.Key.ToString().ToLower() == skillName.ToLower())
            {
                AddXP(pair.Key, amount, true);
                return true;
            }
        }
        return false;
    }


    // ---------------------------------------------------------
    // Loading Data
    // ---------------------------------------------------------
    public void LoadSkillData(PlayerSkill skill, int level, float currentXP)
    {
        if (skills.ContainsKey(skill))
        {
            SkillData data = skills[skill];
            data.level = level;
            data.currentXP = currentXP;

            // Very important: update the threshold so the UI/Logic knows 
            // when the next level-up actually happens.
            data.RecalculateXPRequirement();

            Debug.Log($"Restored {skill}: Level {level}, XP {currentXP}");
        }
    }

    public void SetSkillLevelInitial(PlayerSkill skill, int level)
    {
        if (level <= 0) return;
    
        SkillData data = skills[skill];
        data.level = level;
        data.currentXP = 0;
        data.RecalculateXPRequirement();
    }
}
