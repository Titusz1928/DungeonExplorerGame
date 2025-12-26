using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InjuryRowPrefab : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button treatButton;

    [Header("Treat Window Settings")]
    [SerializeField] private GameObject treatInjuryWindowPrefab; // Assign the prefab here

    private Injury linkedInjury;

    public void SetData(Injury injury)
    {
        linkedInjury = injury;

        // 1. Set Text Fields
        locationText.text = injury.bodyPart.ToString();
        typeText.text = injury.type.ToString();

        // 2. Determine Status
        if (injury.isBandaged)
        {
            statusText.text = injury.bandageDirty ? "Dirty Bandage" : "Bandaged";
            statusText.color = injury.bandageDirty ? Color.yellow : Color.cyan;
        }
        else
        {
            statusText.text = "Bleeding";
            statusText.color = Color.red;
        }

        // 3. Handle Treat Button
        // Now interactive!
        treatButton.interactable = true;

        // Remove old listeners and add the new one
        treatButton.onClick.RemoveAllListeners();
        treatButton.onClick.AddListener(OnTreatClicked);

        var colors = treatButton.colors;
        colors.normalColor = injury.severity > 50 ? Color.red : Color.white;
        treatButton.colors = colors;
    }

    private void OnTreatClicked()
    {
        if (treatInjuryWindowPrefab != null)
        {
            // 1. Open window
            GameObject windowInstance = WindowManager.Instance.OpenWindow(treatInjuryWindowPrefab);

            // 2. Pass the specific LINKED INJURY instance
            windowInstance.GetComponent<TreatInjuryWindow>().OpenTreatWindow(linkedInjury);
        }
    }
}