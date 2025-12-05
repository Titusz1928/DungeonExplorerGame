using UnityEngine;
using UnityEngine.UI;

public class MovementButtonsUI : MonoBehaviour
{
    [Header("Buttons Background Icons")]
    public Image sneakIcon;
    public Image sprintIcon;

    [Header("References")]
    public PlayerStateManager state;

    private void OnEnable()
    {
        state.OnMovementModeChanged += UpdateVisuals;
    }

    private void OnDisable()
    {
        state.OnMovementModeChanged -= UpdateVisuals;
    }

    public void OnSneakButtonPressed()
    {
        bool enableSneak = state.CurrentMode != MovementMode.Sneaking;
        state.SetSneak(enableSneak);
    }

    public void OnSprintButtonPressed()
    {
        bool enableSprint = state.CurrentMode != MovementMode.Sprinting;
        state.SetSprint(enableSprint);
    }

    private void UpdateVisuals(MovementMode mode)
    {
        switch (mode)
        {
            case MovementMode.Sneaking:
                SetVisible(sneakIcon, true);
                SetVisible(sprintIcon, false);
                break;

            case MovementMode.Sprinting:
                SetVisible(sneakIcon, false);
                SetVisible(sprintIcon, true);
                break;

            default: // normal
                SetVisible(sneakIcon, false);
                SetVisible(sprintIcon, false);
                break;
        }
    }

    private void SetVisible(Image img, bool visible)
    {
        img.color = visible ? Color.white : new Color(1, 1, 1, 0);
    }
}
