using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Mobile Controls")]
    public Joystick joystick;  // Assign your joystick in the Inspector

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerControls controls;
    private Animator animator;
    private bool usingJoystick = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Setup Input System
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx =>
        {
            // Only use keyboard input if not currently using joystick
            if (!usingJoystick)
            {
                moveInput = ctx.ReadValue<Vector2>();
            }
        };
        controls.Player.Move.canceled += ctx =>
        {
            // Only zero keyboard input if not currently using joystick
            if (!usingJoystick)
            {
                moveInput = Vector2.zero;
            }
        };
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // --- Get joystick input ---
        Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);

        // Check if joystick is being used
        if (joystickInput.magnitude > 0.1f)
        {
            usingJoystick = true;
            moveInput = joystickInput;
        }
        else if (usingJoystick)
        {
            // Joystick was being used but is now released
            usingJoystick = false;
            moveInput = Vector2.zero;
        }
        // If not using joystick, keyboard input is handled by the Input System callbacks

        // --- Animator ---
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
        // --- Move player ---
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }
}