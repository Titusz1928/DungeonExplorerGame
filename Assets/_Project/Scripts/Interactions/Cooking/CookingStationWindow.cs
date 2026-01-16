using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingStationWindow : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI stationNameText;
    public Image stationPreviewImage;
    public Button lightFireButton;
    public Image fuelSliderFill;

    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI statusText;

    private CookingStation currentStation;


    [Header("Cooking Grid")]
    public Transform gridParent;      // The Content object with VerticalLayoutGroup
    public GameObject rowPrefab;      // Prefab with HorizontalLayoutGroup
    public GameObject cellPrefab;     // Prefab with CookableCell script
    public int maxItemsPerRow = 5;

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

        DrawCookableGrid();

        Debug.Log($"[UI] Cooking Window opened for station: {station.name}");

        // In the future, you'd update fuel sliders or item slots here
    }

    public void DrawCookableGrid()
    {
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        if (currentStation == null) return;
        var container = currentStation.GetComponent<WorldContainer>();
        if (container == null) return;

        Transform currentRow = null;
        int cellsInCurrentRow = 0;

        foreach (var item in container.items)
        {
            if (item.itemSO == null || item.itemSO.isFuel) continue;

            // Unstacking visually
            for (int q = 0; q < item.quantity; q++)
            {
                if (cellsInCurrentRow == 0 || cellsInCurrentRow >= maxItemsPerRow)
                {
                    currentRow = Instantiate(rowPrefab, gridParent).transform;
                    cellsInCurrentRow = 0;
                }

                GameObject cellGO = Instantiate(cellPrefab, currentRow);
                CookableCell cell = cellGO.GetComponent<CookableCell>();

                if (cell != null)
                {
                    // We pass the instance so the cell can read 'cookingProgress'
                    cell.Setup(item, currentStation);
                }

                cellsInCurrentRow++;
            }
        }
    }

    void Update()
    {
        if (currentStation == null) return;

        // 1. CHECK FOR TRANSFORMATIONS
        // If the station says data changed (Raw -> Cooked), we must redraw the icons
        if (currentStation.inventoryUpdated)
        {
            DrawCookableGrid();
            currentStation.inventoryUpdated = false; // Reset the flag
        }

        // 2. Standard Updates
        float totalFuel = currentStation.GetTotalPotentialFuel();
        statusText.text = currentStation.isOnFire ? "LIT" : "COLD";

        if (stationPreviewImage != null)
        {
            stationPreviewImage.sprite = currentStation.isOnFire ?
                currentStation.StationOnFireSprite : currentStation.StationNotOnFireSprite;
        }

        if (totalFuel <= 0)
        {
            fuelText.text = "NO FUEL";
            if (fuelSliderFill != null) fuelSliderFill.fillAmount = 0;
        }
        else
        {
            fuelText.text = currentStation.isOnFire ?
                $"Burning: {FormatTime(totalFuel)}" :
                $"Potential: {FormatTime(totalFuel)}";

            if (fuelSliderFill != null)
                fuelSliderFill.fillAmount = totalFuel / currentStation.maxFuelCapacity;
        }
    }

    // Helper to make the time look nice (00:00)
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, secs);
    }

    public void ToggleFire()
    {
        if (currentStation == null) return;

        // Prevention check: Don't light if no fuel
        if (!currentStation.isOnFire && currentStation.GetTotalPotentialFuel() <= 0)
        {
            return;
        }

        currentStation.isOnFire = !currentStation.isOnFire;

        // Update the world object sprite immediately
        currentStation.UpdateWorldVisuals();

        // Optional: If you want the UI preview image to change too:
        if (stationPreviewImage != null)
        {
            stationPreviewImage.sprite = currentStation.isOnFire ?
                currentStation.StationOnFireSprite : currentStation.StationNotOnFireSprite;
        }
    }
}