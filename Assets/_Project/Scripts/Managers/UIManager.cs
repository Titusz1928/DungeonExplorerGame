using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public static MovementButtonsUI sprintButton;

    [Header("Gameplay UI Elements")]
    public GameObject joystickUI;

    private int windowCount = 0; // Tracks how many windows are open

    public bool IsWindowOpen => windowCount > 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetWindowState(bool opening)
    {
        if (opening)
            windowCount++;
        else
            windowCount--;

        // Safety: don't let it drop below 0
        windowCount = Mathf.Max(0, windowCount);

        // Toggle gameplay UI (joystick, etc.)
        if (joystickUI != null)
            joystickUI.SetActive(!IsWindowOpen);
    }

}
