using UnityEngine;

public enum MovementMode { Normal, Sneaking, Sprinting }

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public MovementMode CurrentMode { get; private set; } = MovementMode.Normal;

    public float stamina = 100f;
    public float health = 100f;

    public event System.Action<MovementMode> OnMovementModeChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetSneak(bool active)
    {
        if (active)
            SetMode(MovementMode.Sneaking);
        else
            SetMode(MovementMode.Normal);
    }

    public void SetSprint(bool active)
    {
        if (active)
            SetMode(MovementMode.Sprinting);
        else
            SetMode(MovementMode.Normal);
    }

    private void SetMode(MovementMode newMode)
    {
        CurrentMode = newMode;
        OnMovementModeChanged?.Invoke(newMode);
    }
}
