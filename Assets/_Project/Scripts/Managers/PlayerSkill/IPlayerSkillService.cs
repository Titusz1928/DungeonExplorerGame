public interface IPlayerSkillService
{
    int GetLevel(PlayerSkill skill);
    float GetXP(PlayerSkill skill);
    float GetXPToNext(PlayerSkill skill);
    SkillData GetSkillData(PlayerSkill skill);
    void AddXP(PlayerSkill skill, float baseAmount, bool notifyPlayer);
    bool TryAddXP(string skillName, float amount);
    void LoadSkillData(PlayerSkill skill, int level, float currentXP);
    void SetSkillLevelInitial(PlayerSkill skill, int level);
}