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
        // Try to find the player state if it's not assigned
        if (state == null)
        {
            FindPlayerState();
        }

        // Only subscribe if we successfully found the state
        if (state != null)
        {
            state.OnMovementModeChanged += UpdateVisuals;
            UpdateVisuals(state.CurrentMode); // Sync UI immediately on enable
        }
    }

    private void OnDisable()
    {
        if (state != null)
        {
            state.OnMovementModeChanged -= UpdateVisuals;
        }
    }

    private void FindPlayerState()
    {
        // Use your existing PlayerReference static property
        if (PlayerReference.PlayerTransform != null)
        {
            state = PlayerReference.PlayerTransform.GetComponent<PlayerStateManager>();
        }
        else
        {
            // Fallback: If PlayerReference isn't set yet, look for the object by name or tag
            GameObject player = GameObject.Find("PLAYER");
            if (player != null)
            {
                state = player.GetComponent<PlayerStateManager>();
            }
        }
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
