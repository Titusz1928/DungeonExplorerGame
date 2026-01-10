using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillTokenCell : MonoBehaviour
{
    public TextMeshProUGUI skillNameLabel;
    public TextMeshProUGUI levelLabel;
    public Button plusButton;
    public Button minusButton;

    private PlayerSkill currentSkill;
    private int currentLevel = 0;

    public void Setup(PlayerSkill skill)
    {
        currentSkill = skill;
        skillNameLabel.text = skill.ToString();
        UpdateUI();

        plusButton.onClick.AddListener(() => GameSessionController.Instance.AdjustSkill(currentSkill, 1));
        minusButton.onClick.AddListener(() => GameSessionController.Instance.AdjustSkill(currentSkill, -1));
    }

    public void UpdateUI()
    {
        currentLevel = GameSessionController.Instance.GetSelectedLevel(currentSkill);
        levelLabel.text = currentLevel.ToString();
    }
}