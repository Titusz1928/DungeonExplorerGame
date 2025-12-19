using System.Collections.Generic;

public static class SkillSaveBuilder
{
    public static List<SkillSaveEntry> Build()
    {
        List<SkillSaveEntry> save = new List<SkillSaveEntry>();

        foreach (PlayerSkill skill in System.Enum.GetValues(typeof(PlayerSkill)))
        {
            //if (skill == PlayerSkill.None) // optional if you have a None enum
            //    continue;

            SkillData data = PlayerSkillManager.Instance.GetSkillData(skill);

            save.Add(new SkillSaveEntry
            {
                skill = skill,
                level = data.level,
                xp = data.currentXP
            });
        }

        return save;
    }

    public static void Apply(List<SkillSaveEntry> savedSkills)
    {
        if (savedSkills == null) return;

        var skillManager = PlayerSkillManager.Instance;

        foreach (var entry in savedSkills)
        {
            // We assume PlayerSkillManager has a method like LoadSkill
            // to set values directly instead of using AddXP()
            skillManager.LoadSkillData(entry.skill, entry.level, entry.xp);
        }
    }
}
