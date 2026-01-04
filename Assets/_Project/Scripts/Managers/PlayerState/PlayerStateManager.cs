using System.Collections;
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


    [Header("Health Settings")]
    public float maxHealth = 100f;
    public Bar healthBar;
    public GameObject healthBarbackground;
    public GameObject gameOverWindow;
    public bool isDead = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void inflictDamage(float amount)
    {
        // Subtract damage
        health -= amount;

        // Clamp health so it doesn't stay as a weird negative number
        health = Mathf.Max(health, 0);

        // Check for death
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died!");

        // Show the game over UI immediately
        if (gameOverWindow != null)
        {
            WindowManager.Instance.OpenWindow(gameOverWindow);
        }

        // Start the timer to change the scene
        StartCoroutine(WaitAndLoadMenu());
    }

    private IEnumerator WaitAndLoadMenu()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Load the scene using your SceneManagerEX
        SceneManagerEX.Instance.LoadScene("MainMenu");
    }

    public void addHealth(float amount)
    {
        if(health+amount> maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health = health + amount;
        }
    }

    public void healMax()
    {
        health = maxHealth;
    }

    public void addStamina(float amount)
    {
        stamina = Mathf.Clamp(stamina + amount, 0f, maxStamina);
    }

    public void staminaMax()
    {
        stamina = maxStamina;
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

        if (healthBar != null)
        {
            // Show only when NOT full
            healthBar.gameObject.SetActive(health < maxHealth);
            healthBar.gameObject.SetActive(health < maxHealth);

            // Update fill
            healthBar.SetValue(health / maxHealth);
        }
    }

}
