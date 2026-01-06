using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Window Roots")]
    [SerializeField] private Transform generalWindowRoot; // In GeneralCanvas
    [SerializeField] private Transform battleWindowRoot;  // In BattleCanvas

    [Header("Canvases")]
    [SerializeField] private Canvas generalCanvas; // Your exploration/main UI
    [SerializeField] private Canvas battleCanvas;  // Your turn-based combat UI

    [Header("Gameplay UI Elements")]
    public GameObject joystickUI;

    private int windowCount = 0;

    public bool IsWindowOpen => windowCount > 0;
    public bool IsInBattle { get; private set; } // Tracks current state

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialization: Start in Exploration mode
        InitializeUI();
    }

    private void InitializeUI()
    {
        IsInBattle = false;
        generalCanvas.enabled = true;
        battleCanvas.enabled = false;
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
}