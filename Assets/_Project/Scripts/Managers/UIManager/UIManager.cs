using TitusGames.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IUIManagerService
{
    public GameObject GameObject => gameObject;

    [Header("Window Roots")]
    [SerializeField] private Transform generalWindowRoot; // In GeneralCanvas
    [SerializeField] private Transform battleWindowRoot;  // In BattleCanvas

    [Header("Canvases")]
    [SerializeField] private Canvas generalCanvas; // Your exploration/main UI
    [SerializeField] private Canvas battleCanvas;  // Your turn-based combat UI

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera; // Drag your Main Camera here
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 7f;

    [Header("ExtraHUD")]
    [SerializeField] private TextMeshProUGUI playerCoordinatesText;

    [SerializeField] private GameObject cheatsButton;
    [SerializeField] private GameObject battlecheatsButton;

    [Header("Gameplay UI Elements")]
    public GameObject joystickUI;
    public Image VisibilityImage;

    private IWindowService windowService;
    private bool wasWindowOpen = false;

    // Ask the framework directly if windows are open
    public bool IsWindowOpen => windowService != null && windowService.WindowCount > 0;
    public bool IsInBattle { get; private set; }

    private void Awake()
    {
        ServiceLocator.Current.Register<IUIManagerService>(this);

        if (playerCoordinatesText != null)
            playerCoordinatesText.gameObject.SetActive(false);

        if (GameSettings.Instance != null)
        {
            if(GameSettings.Instance.IsVisible())
                VisibilityImage.gameObject.SetActive(false);
        }

        // Initialization: Start in Exploration mode
        InitializeUI();
    }

    private void Update()
    {
        // Monitor for window state changes automatically without needing package events
        if (windowService != null)
        {
            bool isWindowOpenNow = windowService.WindowCount > 0;

            // If the state changed this frame (a window opened or all closed), update the joystick
            if (isWindowOpenNow != wasWindowOpen)
            {
                wasWindowOpen = isWindowOpenNow;
                UpdateJoystickVisibility();
            }
        }
    }

    private void Start()
    {
        // Grab the framework's window service once at the start
        windowService = ServiceLocator.Current.Get<IWindowService>();
    }

    private void OnDestroy()
    {
        ServiceLocator.Current.Unregister<IUIManagerService>();
    }

    private void InitializeUI()
    {
        IsInBattle = false;
        generalCanvas.enabled = true;
        battleCanvas.enabled = false;

        if (GameSettings.Instance != null)
        {
            cheatsButton.SetActive(GameSettings.Instance.CheatsEnabled);
            battlecheatsButton.SetActive(GameSettings.Instance.CheatsEnabled);
        }

        UpdateJoystickVisibility();
    }

    // --- State Switching ---

    public void EnterBattleState()
    {
        IsInBattle = true;
        generalCanvas.enabled = false; // Or keep true if you want background UI
        battleCanvas.enabled = true;

        if (windowService != null) windowService.RegisterUIRoot(battleWindowRoot);
        UpdateJoystickVisibility();
    }

    public void ExitBattleState()
    {
        IsInBattle = false;
        generalCanvas.enabled = true;
        battleCanvas.enabled = false;

        if (windowService != null) windowService.RegisterUIRoot(generalWindowRoot);
        UpdateJoystickVisibility();
    }

    // --- Window Logic ---

    public void SetWindowState(bool opening)
    {
        UpdateJoystickVisibility();
    }

    private void UpdateJoystickVisibility()
    {
        if (joystickUI == null) return;

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
        VisibilityImage.gameObject.SetActive(!GameSettings.Instance.IsVisible());
    }
}