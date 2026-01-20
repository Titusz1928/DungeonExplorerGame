using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Window Roots")]
    [SerializeField] private Transform generalWindowRoot; // In GeneralCanvas
    [SerializeField] private Transform battleWindowRoot;  // In BattleCanvas

    [Header("Canvases")]
    [SerializeField] private Canvas generalCanvas; // Your exploration/main UI
    [SerializeField] private Canvas battleCanvas;  // Your turn-based combat UI

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera; // Drag your Main Camera here
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 7f;

    [Header("ExtraHUD")]
    [SerializeField] private TextMeshProUGUI playerCoordinatesText;

    [SerializeField] private GameObject cheatsButton;

    [Header("Gameplay UI Elements")]
    public GameObject joystickUI;

    public Image VisibilityImage;

    private int windowCount = 0;

    public bool IsWindowOpen => windowCount > 0;
    public bool IsInBattle { get; private set; } // Tracks current state

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if(playerCoordinatesText != null)
            playerCoordinatesText.gameObject.SetActive(false);

        if (GameSettings.Instance != null)
        {
            if(GameSettings.Instance.IsVisible())
                VisibilityImage.gameObject.SetActive(false);
        }

        // Initialization: Start in Exploration mode
        InitializeUI();
    }

    private void InitializeUI()
    {
        IsInBattle = false;
        generalCanvas.enabled = true;
        battleCanvas.enabled = false;

        if (GameSettings.Instance != null)
        {
            if(GameSettings.Instance.CheatsEnabled)
                cheatsButton.SetActive(true);
            else
                cheatsButton.SetActive(false);
        }

        UpdateJoystickVisibility();
    }

    // --- State Switching ---

    public void EnterBattleState()
    {
        IsInBattle = true;
        generalCanvas.enabled = false; // Or keep true if you want background UI
        battleCanvas.enabled = true;

        WindowManager.Instance.RegisterUIRoot(battleWindowRoot);

        UpdateJoystickVisibility();
    }

    public void ExitBattleState()
    {
        IsInBattle = false;
        generalCanvas.enabled = true;
        battleCanvas.enabled = false;

        WindowManager.Instance.RegisterUIRoot(generalWindowRoot);

        UpdateJoystickVisibility();
    }

    // --- Window Logic ---

    public void SetWindowState(bool opening)
    {
        windowCount = opening ? windowCount + 1 : windowCount - 1;
        windowCount = Mathf.Max(0, windowCount);

        UpdateJoystickVisibility();
    }

    private void UpdateJoystickVisibility()
    {
        if (joystickUI == null) return;

        // The joystick should ONLY be active if:
        // 1. No windows are open AND 2. We are NOT in a battle
        bool shouldShowJoystick = !IsWindowOpen && !IsInBattle;
        joystickUI.SetActive(shouldShowJoystick);
    }


    // --- Zoom Functions for Buttons ---

    public void ZoomIn()
    {
        if (mainCamera == null) return;

        // Orthographic: Smaller size = Closer/Zoomed In
        float newSize = mainCamera.orthographicSize - 1f;
        mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    public void ZoomOut()
    {
        if (mainCamera == null) return;

        // Orthographic: Larger size = Further/Zoomed Out
        float newSize = mainCamera.orthographicSize + 1f;
        mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    public void deactivateExtraHUD()
    {
        playerCoordinatesText.gameObject.SetActive(false);
    }

    public void activateExtraHUD()
    {
        playerCoordinatesText.gameObject.SetActive(true);
    }

    public void changeVisibilityUIStatus()
    {
        GameSettings.Instance.ToggelVisibility();

        if (GameSettings.Instance.IsVisible())
        {
            VisibilityImage.gameObject.SetActive(false);
        }
        else
        {
            VisibilityImage.gameObject.SetActive(true);
        }
    }
}