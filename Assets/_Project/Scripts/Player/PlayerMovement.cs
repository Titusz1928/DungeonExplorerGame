using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 3.5f;

    [Header("Movement Multipliers")]
    public float sneakMultiplier = 0.5f;
    public float sprintMultiplier = 1.5f;

    [Header("Mobile Controls")]
    private Joystick joystick;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerControls controls;
    private Animator animator;

    private bool usingJoystick = false;

    private PlayerStateManager state;
    private float currentSpeed;

    //for skill levels and noise
    private Vector3 lastPosition;
    private float xpDistanceAccumulator = 0f;
    private float noiseDistanceAccumulator = 0f;

    // How often to award XP per meters walked
    [Header("Xp values")]
    public float metersPerXP = 10f;
    public float xpPerTick = 0.5f;

    [Header("Noise Settings")]
    public float metersPerNoise = 1f;

    [Header("Battle Detection")]
    [SerializeField] private float battleDetectionRadius = 0.5f; // Tiny radius around player
    [SerializeField] private LayerMask enemyLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        state = PlayerStateManager.Instance;

        controls = new PlayerControls();
        controls.Player.Move.performed += ctx =>
        {
            if (!usingJoystick)
                moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            if (!usingJoystick)
                moveInput = Vector2.zero;
        };
    }

    private void Start()
    {
        lastPosition = transform.position;
        if (state == null) state = GetComponent<PlayerStateManager>();
        if (state != null)
        {
            UpdateSpeed(state.CurrentMode);
            state.OnMovementModeChanged += UpdateSpeed;
        }
        FindJoystick();
    }

    private void OnEnable()
    {
        controls.Enable();
        // Re-link the joystick every time a scene is loaded
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        controls.Disable();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        FindJoystick();
    }

    private void FindJoystick()
    {
        // Look for the Joystick component in the new scene
        joystick = FindFirstObjectByType<Joystick>();
    }

    public void UpdateSpeed(MovementMode mode)
    {
        // 1. Get skill levels
        int speedLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Speed);
        int stealthLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Stealth);
        int strengthLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Strength);

        // 2. Calculate initial base and multipliers
        float effectiveBaseSpeed = baseSpeed + (speedLevel * 0.03f);
        float effectiveSneakMultiplier = sneakMultiplier + (stealthLevel * 0.01f);
        float effectiveSprintMultiplier = sprintMultiplier + (strengthLevel * 0.05f);

        // 3. --- CALCULATE INJURY PENALTY ---
        float speedModifier = 1.0f;
        var injuryManager = GetComponent<InjuryManager>();

        if (injuryManager != null)
        {
            foreach (var injury in injuryManager.activeInjuries)
            {
                // If the injury is on a "walking" part
                if (injury.bodyPart == ArmorSlot.Legs || injury.bodyPart == ArmorSlot.Feet)
                {
                    // Example: A severity 100 injury on legs reduces speed by 40% (0.4)
                    // We use Mathf.Clamp to ensure we don't go below 0 speed
                    float penalty = (injury.severity / 100f) * 0.4f;
                    speedModifier -= penalty;
                }
            }
        }
        // Cap the modifier so the player can always at least move a little (e.g., 20% speed)
        speedModifier = Mathf.Max(0.2f, speedModifier);

        switch (mode)
        {
            case MovementMode.Sneaking:
                currentSpeed = effectiveBaseSpeed * effectiveSneakMultiplier;
                break;

            case MovementMode.Sprinting:
                currentSpeed = effectiveBaseSpeed * effectiveSprintMultiplier;
                break;

            default:
                currentSpeed = effectiveBaseSpeed;
                break;
        }

        currentSpeed *= speedModifier;
    }

    private void Update()
    {
        if (UIManager.Instance != null && (UIManager.Instance.IsWindowOpen || UIManager.Instance.IsInBattle))
        {
            moveInput = Vector2.zero;
            usingJoystick = false;
            state.IsMoving = false;
            return;
        }

        CheckForNearbyEnemies();

        // --- Joystick input ---
        if (joystick != null)
        {
            Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);

            if (joystickInput.magnitude > 0.1f)
            {
                usingJoystick = true;
                moveInput = joystickInput;
            }
            else if (usingJoystick)
            {
                usingJoystick = false;
                moveInput = Vector2.zero;
            }

            // --- Animator stuff ---
            bool walking = moveInput.magnitude > 0.1f;
            state.IsMoving = walking;
            animator.SetBool("isWalking", walking);

            if (walking)
            {
                animator.SetFloat("InputX", moveInput.x);
                animator.SetFloat("InputY", moveInput.y);
                animator.SetFloat("LastInputX", moveInput.x);
                animator.SetFloat("LastInputY", moveInput.y);
            }

            // --- XP Gain For Movement ---
            if (walking)   // use local walking, not previous frame's state
            {
                float dist = Vector3.Distance(transform.position, lastPosition);
                xpDistanceAccumulator += dist;

                if (xpDistanceAccumulator >= metersPerXP)
                {
                    xpDistanceAccumulator = 0f;

                    // --- XP ---
                    PlayerSkillManager.Instance.AddXP(PlayerSkill.Speed, xpPerTick, true);

                    if (state.CurrentMode == MovementMode.Sneaking)
                        PlayerSkillManager.Instance.AddXP(PlayerSkill.Stealth, xpPerTick * 1.2f, true);

                    if (state.CurrentMode == MovementMode.Sprinting)
                        PlayerSkillManager.Instance.AddXP(PlayerSkill.Strength, xpPerTick * 0.5f, true);

                }

                // --- Noise ---
                noiseDistanceAccumulator += dist;
                if (noiseDistanceAccumulator >= metersPerNoise)
                {
                    noiseDistanceAccumulator = 0f;

                    switch (state.CurrentMode)
                    {
                        case MovementMode.Sneaking:
                            break;
                        case MovementMode.Normal:
                            NoiseManager.Instance.EmitActionNoise(NoiseActionType.Walking, transform.position);
                            break;
                        case MovementMode.Sprinting:
                            NoiseManager.Instance.EmitActionNoise(NoiseActionType.Sprinting, transform.position);
                            break;
                    }
                }
            }
        }

        // Update last position AFTER xp logic
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (UIManager.Instance != null && (UIManager.Instance.IsWindowOpen || UIManager.Instance.IsInBattle))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // --- Move player with state-based speed ---
        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }

    private void CheckForNearbyEnemies()
    {
        // Level 3: Is the visibility setting blocking us?
        if (!GameSettings.Instance.IsVisible())
        {
            // If this prints, your "Cheat/Settings" are hiding the player from the code!
            // Debug.Log("Check failed: Player is currently INVISIBLE per GameSettings.");
            return;
        }

        // Level 4: The Physics Check
        // We will temporarily REMOVE the enemyLayer filter to see if it finds ANYTHING
        Collider2D hit = Physics2D.OverlapCircle(transform.position, battleDetectionRadius);

        if (hit != null)
        {
            Debug.Log($"PHYSICS HIT: Found {hit.name} on Layer: {LayerMask.LayerToName(hit.gameObject.layer)} with Tag: {hit.tag}");

            if (hit.CompareTag("Enemy"))
            {
                StartBattle(hit.gameObject);
            }
        }
    }

    private void StartBattle(GameObject enemy)
    {
        EnemyController mainEnemy = enemy.GetComponent<EnemyController>();
        if (mainEnemy == null) return;

        // 1. Switch UI to Battle Mode
        WindowManager.Instance.CloseAllWindows();

        UIManager.Instance.EnterBattleState();

        // 2. Find all enemies within 15 units
        float searchRadius = 10f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        List<EnemyController> potentialHelpers = new List<EnemyController>();

        foreach (var hit in hitColliders)
        {
            // Skip the enemy we actually bumped into (he's already the main target)
            if (hit.gameObject == enemy) continue;

            if (hit.CompareTag("Enemy"))
            {
                EnemyController ec = hit.GetComponent<EnemyController>();
                if (ec != null)
                {
                    // 3. Filter by State: Investigating, Searching, or Chasing
                    EnemyState s = ec.GetState();
                    if (s == EnemyState.Investigating || s == EnemyState.Searching || s == EnemyState.Chasing)
                    {
                        potentialHelpers.Add(ec);
                    }
                }
            }
        }

        // 4. Sort by distance and take only the 5 closest
        List<EnemyController> finalHelpers = potentialHelpers
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .Take(5)
            .ToList();

        // 5. Send to BattleManager
        BattleManager.Instance.StartBattle(mainEnemy, finalHelpers);

        // 6. Handle exploration world cleanup
        // Instead of just Destroying, we usually disable them so we can return to them if we flee
        //mainEnemy.gameObject.SetActive(false);
        //foreach (var helper in finalHelpers)
        //{
        //    helper.gameObject.SetActive(false);
        //}

        //Destroy(enemy);


    }

    private void OnDrawGizmosSelected()
    {
        // This allows you to see the detection radius in the Scene view when you click the player
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, battleDetectionRadius);
    }
}
