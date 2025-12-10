using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 4f;

    [Header("Movement Multipliers")]
    public float sneakMultiplier = 0.5f;
    public float sprintMultiplier = 1.2f;

    [Header("Mobile Controls")]
    public Joystick joystick;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerControls controls;
    private Animator animator;

    private bool usingJoystick = false;

    private PlayerStateManager state;
    private float currentSpeed;

    //for skill levels
    private Vector3 lastPosition;
    private float distanceAccumulator = 0f;

    // How often to award XP per meters walked
    [Header("Xp values")]
    public float metersPerXP = 10f;
    public float xpPerTick = 0.5f;

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

        UpdateSpeed(state.CurrentMode);
        state.OnMovementModeChanged += UpdateSpeed;  // ⭐ Listen for changes
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void UpdateSpeed(MovementMode mode)
    {
        // --- Get skill levels ---
        int speedLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Speed);
        int stealthLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Stealth);
        int strengthLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Strength);

        // --- Base speed scales WEAKLY with Speed Skill ---
        float effectiveBaseSpeed = baseSpeed + (speedLevel * 0.03f);

        // --- Sneak scaling (VERY small) ---
        float effectiveSneakMultiplier = sneakMultiplier + (stealthLevel * 0.01f);

        // --- Sprint scaling (stronger, but safe) ---
        float effectiveSprintMultiplier = sprintMultiplier + (strengthLevel * 0.05f);

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
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsWindowOpen)
        {
            moveInput = Vector2.zero;
            usingJoystick = false;
            state.IsMoving = false;
            return;
        }

        // --- Joystick input ---
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
            distanceAccumulator += dist;

            if (distanceAccumulator >= metersPerXP)
            {
                distanceAccumulator = 0f;
                //Debug.Log("xp is given!");
                // XP: Speed
                PlayerSkillManager.Instance.AddXP(PlayerSkill.Speed, xpPerTick);

                // XP: Stealth
                if (state.CurrentMode == MovementMode.Sneaking)
                    PlayerSkillManager.Instance.AddXP(PlayerSkill.Stealth, xpPerTick * 1.2f);

                // XP: Strength
                if (state.CurrentMode == MovementMode.Sprinting)
                    PlayerSkillManager.Instance.AddXP(PlayerSkill.Strength, xpPerTick * 0.5f);
            }
        }

        // Update last position AFTER xp logic
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsWindowOpen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // --- Move player with state-based speed ---
        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }
}
