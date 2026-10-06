using System.Collections;
using UnityEngine;
using TitusGames.Framework; // Adjust namespace as needed for your framework

public enum MovementMode { Normal, Sneaking, Sprinting }

public class PlayerStateManager : MonoBehaviour, IPlayerStateService
{
    public MovementMode CurrentMode { get; private set; } = MovementMode.Normal;

    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    [Header("Stamina Settings")]
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;
    public float staminaRegenPerSecond = 15f;
    public Bar staminaBar;
    public GameObject staminaBarbackground;

    [Header("Health Settings")]
    public float health = 100f;
    public float maxHealth = 100f;
    public Bar healthBar;
    public GameObject healthBarbackground;
    public GameObject gameOverWindow;

    public bool IsMoving { get; set; }
    public bool IsDead { get; private set; }

    // Public explicit interface property getters
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public float Health => health;
    public float MaxHealth => maxHealth;

    public event System.Action<MovementMode> OnMovementModeChanged;

    private void OnEnable()
    {
        ServiceLocator.Current.Register<IPlayerStateService>(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Current.Unregister<IPlayerStateService>();
    }

    // --- HEALTH MANAGEMENT ---

    public void InflictDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0f);

        if (health <= 0f && !IsDead)
        {
            Die();
        }
    }

    public void AddHealth(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    public void HealMax()
    {
        health = maxHealth;
    }

    // --- STAMINA MANAGEMENT ---

    public void AddStamina(float amount)
    {
        stamina = Mathf.Clamp(stamina + amount, 0f, maxStamina);
    }

    public void StaminaMax()
    {
        stamina = maxStamina;
    }

    // --- MOVEMENT MODES ---

    public void SetSneak(bool active)
    {
        SetMode(active ? MovementMode.Sneaking : MovementMode.Normal);
    }

    public void SetSprint(bool active)
    {
        if (active && stamina <= 0f) return;
        SetMode(active ? MovementMode.Sprinting : MovementMode.Normal);
    }

    private void SetMode(MovementMode newMode)
    {
        CurrentMode = newMode;
        OnMovementModeChanged?.Invoke(newMode);
    }

    // --- DEATH & SCENE HANDLING ---

    private void Die()
    {
        IsDead = true;
        Debug.Log("Player has died!");

        // Resolved Audio Service
        if (ServiceLocator.Current.TryGet<IGameAudioService>(out var audioService))
        {
            audioService.PlayGameOverSFX();
        }

        // Resolved Window/UI Service
        if (gameOverWindow != null && ServiceLocator.Current.TryGet<IWindowService>(out var windowService))
        {
            windowService.OpenWindow(gameOverWindow);
        }

        StartCoroutine(WaitAndLoadMenu());
    }

    private IEnumerator WaitAndLoadMenu()
    {
        yield return new WaitForSeconds(2f);

        // Resolved Scene Service
        if (ServiceLocator.Current.TryGet<ISceneService>(out var sceneService))
        {
            sceneService.LoadScene("MainMenu");
        }
    }

    // --- UPDATE LOOP ---

    private void Update()
    {
        UpdateStamina();
        UpdateUI();
    }

    private void UpdateStamina()
    {
        if (CurrentMode == MovementMode.Sprinting && IsMoving)
        {
            stamina -= staminaDrainPerSecond * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);

            if (stamina <= 0f)
            {
                SetMode(MovementMode.Normal);
            }
        }
        else
        {
            stamina += staminaRegenPerSecond * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
    }

    private void UpdateUI()
    {
        if (staminaBar != null)
        {
            bool showStamina = stamina < maxStamina;
            staminaBar.gameObject.SetActive(showStamina);
            if (staminaBarbackground != null) staminaBarbackground.SetActive(showStamina);
            staminaBar.SetValue(stamina / maxStamina);
        }

        if (healthBar != null)
        {
            bool showHealth = health < maxHealth;
            healthBar.gameObject.SetActive(showHealth);
            if (healthBarbackground != null) healthBarbackground.SetActive(showHealth);
            healthBar.SetValue(health / maxHealth);
        }
    }
}