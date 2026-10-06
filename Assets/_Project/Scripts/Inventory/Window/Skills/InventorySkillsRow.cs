using TitusGames.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySkillRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image xpBar;
    [SerializeField] private TextMeshProUGUI xpToNextText;

    private PlayerSkill skill;

    public void SetData(PlayerSkill skill)
    {
        this.skill = skill;

        // 1. Name
        skillNameText.text = skill.ToString();

        // 2. Level
        ServiceLocator.Current.TryGet<IPlayerSkillService>(out var playerSkillService);

        int level = playerSkillService.GetLevel(skill);
        levelText.text = $"Lv {level}";

        // 3. XP Bar
        float currentXP = playerSkillService.GetXP(skill);
        float xpToNext = playerSkillService.GetXPToNext(skill);

        float fillAmount = xpToNext > 0 ? currentXP / xpToNext : 0f;
        xpBar.fillAmount = fillAmount;

        // 4. XP to next level text
        xpToNextText.text = $"{currentXP:F0}/{xpToNext:F0}";
    }
}
