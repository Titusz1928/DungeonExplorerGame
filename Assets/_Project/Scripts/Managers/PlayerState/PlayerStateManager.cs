using UnityEngine;

public enum MovementMode { Normal, Sneaking, Sprinting }

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public MovementMode CurrentMode { get; private set; } = MovementMode.Normal;

    public float stamina = 100f;
    public float health = 100f;

    public bool IsMoving { get; set; }

    public event System.Action<MovementMode> OnMovementModeChanged;


    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;
    public float staminaRegenPerSecond = 15f;
    public Bar staminaBar;
    public GameObject staminaBarbackground;

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
        if (active && stamina <= 0f)
            return; // can't sprint

        SetMode(active ? MovementMode.Sprinting : MovementMode.Normal);
    }

    private void SetMode(MovementMode newMode)
    {
        CurrentMode = newMode;
        OnMovementModeChanged?.Invoke(newMode);
    }

    private void Update()
    {
        // ⭐ Only drain stamina if sprinting AND moving
        if (CurrentMode == MovementMode.Sprinting && IsMoving)
        {
            stamina -= staminaDrainPerSecond * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

            if (stamina <= 0f)
            {
                // Stop sprint
                SetMode(MovementMode.Normal);
            }
        }
        else
        {
            // Regen stamina when NOT sprinting OR not moving
            stamina += staminaRegenPerSecond * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }

        if (staminaBar != null)
        {
            // Show only when NOT full
            staminaBar.gameObject.SetActive(stamina < maxStamina);
            staminaBar.gameObject.SetActive(stamina < maxStamina);

            // Update fill
            staminaBar.SetValue(stamina / maxStamina);
        }
    }

}
