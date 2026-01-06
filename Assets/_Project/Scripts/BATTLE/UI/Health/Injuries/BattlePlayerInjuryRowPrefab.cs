using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattlePlayerInjuryRowPrefab : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image bodyPartIcon;
    [SerializeField] private Image injuryTypeIcon;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button treatButton;

    [Header("Data")]
    [SerializeField] private InjuryDatabase injuryDatabase;
    private Injury currentInjury;

    public void SetData(Injury injury)
    {
        currentInjury = injury;


        bodyPartIcon.sprite = injuryDatabase.GetBodyPartSprite(injury.bodyPart);
        injuryTypeIcon.sprite = injury.type switch
        {
            InjuryType.Cut => injuryDatabase.cutIcon,
            InjuryType.Stab => injuryDatabase.stabIcon,
            InjuryType.Fracture => injuryDatabase.fractureIcon,
            _ => injuryDatabase.unknownIcon
        };

        // 2. Set Status Text
        if (injury.isBandaged)
        {
            statusText.text = injury.bandageDirty ? "<color=yellow>Dirty Bandage</color>" : "<color=green>Bandaged</color>";
        }
        else
        {
            statusText.text = "<color=red>Bleeding</color>";
        }

        // 3. Setup Button
        treatButton.onClick.RemoveAllListeners();
        treatButton.onClick.AddListener(OnTreatClicked);
    }

    private void OnTreatClicked()
    {
        // Open the window using your existing prefab
        GameObject windowObj = WindowManager.Instance.OpenWindow(BattleUIManager.Instance.treatInjuryWindow);
        TreatInjuryWindow window = windowObj.GetComponent<TreatInjuryWindow>();

        window.OpenTreatWindow(currentInjury);
    }
}