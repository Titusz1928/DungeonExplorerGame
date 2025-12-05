using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 5f;

    [Header("Movement Multipliers")]
    public float sneakMultiplier = 0.5f;
    public float sprintMultiplier = 1.7f;

    [Header("Mobile Controls")]
    public Joystick joystick;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerControls controls;
    private Animator animator;

    private bool usingJoystick = false;

    private PlayerStateManager state;   // ⭐ Reference to the state manager
    private float currentSpeed;

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
        UpdateSpeed(state.CurrentMode);
        state.OnMovementModeChanged += UpdateSpeed;  // ⭐ Listen for changes
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void UpdateSpeed(MovementMode mode)
    {
        switch (mode)
        {
            case MovementMode.Sneaking:
                currentSpeed = baseSpeed * sneakMultiplier;
                break;

            case MovementMode.Sprinting:
                currentSpeed = baseSpeed * sprintMultiplier;
                break;

            default:
                currentSpeed = baseSpeed;
                break;
        }
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsWindowOpen)
        {
            moveInput = Vector2.zero;
            usingJoystick = false;
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
        animator.SetBool("isWalking", walking);

        if (walking)
        {
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
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
