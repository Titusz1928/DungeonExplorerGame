using TitusGames.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        if (!ServiceLocator.Current.TryGet<IBattleUIService>(out var uiService) ||
        !ServiceLocator.Current.TryGet<IWindowService>(out var windowService))
        {
            Debug.LogWarning("Required services for treating injury not found.");
            return;
        }

        // Open window using resolved services
        GameObject windowObj = windowService.OpenWindow(uiService.treatInjuryWindow);
        if (windowObj != null && windowObj.TryGetComponent<TreatInjuryWindow>(out var window))
        {
            window.OpenTreatWindow(currentInjury);
        }
    }

    /*
    
    private void OnTreatClicked()
{
    // 1. Fetch the window service from the Service Locator
    var windowService = ServiceLocator.Current.Get<IWindowService>();
    
    // 2. Fetch the battle UI service (or manager) from the Service Locator
    var battleUIService = ServiceLocator.Current.Get<IBattleUIService>(); // Or BattleUIManager if registered directly as a class

    // 3. Open the window and initialize it
    GameObject windowObj = windowService.OpenWindow(battleUIService.treatInjuryWindow);
    TreatInjuryWindow window = windowObj.GetComponent<TreatInjuryWindow>();

    window.OpenTreatWindow(currentInjury);
}

     */
}