using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingStationWindow : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI stationNameText;
    public Image stationPreviewImage;
    public Button lightFireButton;

    private CookingStation currentStation;

    public void Initialize(CookingStation station, Sprite stationSprite)
    {
        currentStation = station;

        // Refresh the UI based on the station's data
        if (stationNameText != null)
            stationNameText.text = station.interactText;

        // Display the sprite passed from the world object
        if (stationPreviewImage != null && stationSprite != null)
        {
            stationPreviewImage.sprite = stationSprite;

            // Optional: Maintain aspect ratio if your UI image isn't square
            stationPreviewImage.preserveAspect = true;
        }

        Debug.Log($"[UI] Cooking Window opened for station: {station.name}");

        // In the future, you'd update fuel sliders or item slots here
    }

    public void OnLightFirePressed()
    {
        // Example of the UI talking back to the station
        if (currentStation != null)
        {
            Debug.Log("Attempting to light fire...");
            // currentStation.TryLightFire();
        }
    }
}