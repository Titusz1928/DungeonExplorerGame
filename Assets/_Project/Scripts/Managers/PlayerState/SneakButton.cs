using UnityEngine;
using UnityEngine.UI;

public class SneakButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private PlayerStateManager playerState;

    private bool isActive;

    public void ToggleSneak()
    {
        isActive = !isActive;
        icon.color = isActive ? Color.white : new Color(1, 1, 1, 0); // transparent
        playerState.SetSneak(isActive);
    }
}
