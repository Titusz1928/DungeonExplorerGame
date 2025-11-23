using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;
    private Vector2 moveInput;
    public float speed = 5f;
    private Rigidbody2D rb;

    [Header("Mobile Controls")]
    public Joystick joystick;  // Assign in Inspector

    private Animator animator;

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Keyboard / Input System movement
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void Update()
    {
        // Get joystick input
        Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);

        // Use joystick if it's moved
        if (joystickInput.magnitude > 0.1f)
        {
            moveInput = joystickInput;
        }
        else
        {
            // Joystick released → stop movement
            moveInput = Vector2.zero;
        }

        // Update Animator
        bool walking = moveInput.magnitude > 0.1f;
        animator.SetBool("isWalking", walking);

        if (walking)
        {
            // Walking animation
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);

            // Update last input for idle when movement stops
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }


    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
