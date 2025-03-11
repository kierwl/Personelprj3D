using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Singleton instance for easy access
    public static PlayerController Instance;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 curMovementInput;
    public float jumpPower = 7f;
    public bool canDoubleJump = false;
    public LayerMask groundLayerMask;
    private Coroutine boostCoroutine;
    public float rotationSpeed = 15f;

    [Header("Look")]
    public float lookSensitivity = 1f;
    // Keep existing fields and add these new ones
    [Header("Camera Control")]
    public CameraFollow cameraFollow; // Reference to the CameraFollow script
    private Vector2 lookDelta; // Store the look input

    [Header("Other")]
    public BuffUIManager buffUIManager;

    [HideInInspector]
    public bool canLook = true;
    public Action inventory;
    private Rigidbody rigidbody;
    private Animator animator;
    public bool isSprinting = false;
    public bool isLadder = false;

    // Reference to the camera - we'll use this for movement direction
    private Camera mainCamera;
    private Coroutine doubleJumpCoroutine;
    private Vector3 lastStablePosition;
    private float stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 0.2f;
    private const float STUCK_CHECK_TIME = 0.5f;

    private void Awake()
    {
        // Setup singleton instance
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;
        lastStablePosition = transform.position;
    }

    private void Update()
    {
        HandleAnimations();
        CheckForStuckPlayer();
    }

    private void FixedUpdate()
    {
        Move();

        // Sprint stamina consumption
        if (isSprinting)
        {
            CharacterManager.Instance.Player.condition.UseStamina(15f * Time.deltaTime);
        }
    }

    private void CheckForStuckPlayer()
    {
        // Check if the player is moving
        if (curMovementInput.magnitude > 0.1f)
        {
            // Calculate the actual movement since last frame
            float movementDelta = Vector3.Distance(transform.position, lastStablePosition);

            // If player is trying to move but can't
            if (movementDelta < STUCK_THRESHOLD)
            {
                stuckTimer += Time.deltaTime;

                // If stuck for too long, try to resolve
                if (stuckTimer > STUCK_CHECK_TIME)
                {
                    TryToUnstuckPlayer();
                    stuckTimer = 0f;
                }
            }
            else
            {
                // Player is moving normally
                stuckTimer = 0f;
                lastStablePosition = transform.position;
            }
        }
        else
        {
            // Player is not trying to move
            stuckTimer = 0f;
            lastStablePosition = transform.position;
        }
    }

    private void TryToUnstuckPlayer()
    {
        // Small upward force to help with minor terrain obstacles
        rigidbody.AddForce(Vector3.up * 0.5f, ForceMode.Impulse);

        // Try to move slightly back from the direction player is facing
        Vector3 unstuckDirection = -transform.forward * 0.3f;
        RaycastHit hit;

        // Check if there's space behind the player
        if (!Physics.Raycast(transform.position, -transform.forward, out hit, 0.5f))
        {
            rigidbody.MovePosition(transform.position + unstuckDirection);
        }
    }

    public void EnableDoubleJump(float duration, Sprite buffIcon)
    {
        if (doubleJumpCoroutine != null)
            StopCoroutine(doubleJumpCoroutine);

        doubleJumpCoroutine = StartCoroutine(DoubleJumpCoroutine(duration, buffIcon));
    }

    // This method gets called by the Input System when mouse/look input changes
    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Store the look input if we're allowed to look around
        if (canLook && Cursor.lockState == CursorLockMode.Locked)
        {
            lookDelta = context.ReadValue<Vector2>();

            // If we have a reference to the camera follow script
            if (cameraFollow != null)
            {
                // Forward the look input to the camera system
                cameraFollow.UpdateCameraRotation(lookDelta.x * lookSensitivity,
                                                 lookDelta.y * lookSensitivity);
            }
        }
        else
        {
            lookDelta = Vector2.zero;
        }
    }
    // Add this method to properly get camera-based movement direction
    public Vector3 GetCameraRelativeMovementDirection(Vector2 movementInput)
    {
        if (cameraFollow == null || movementInput.magnitude < 0.1f)
            return Vector3.zero;

        // Get forward and right directions from the camera, removing y component
        Vector3 forward = cameraFollow.transform.forward;
        Vector3 right = cameraFollow.transform.right;

        forward.y = 0;
        right.y = 0;

        if (forward.magnitude > 0.01f && right.magnitude > 0.01f)
        {
            forward.Normalize();
            right.Normalize();
        }
        else
        {
            // Fallback if camera is looking directly up/down
            forward = Vector3.forward;
            right = Vector3.right;
        }

        // Calculate movement direction relative to camera
        Vector3 direction = forward * movementInput.y + right * movementInput.x;

        // Normalize for consistent speed in all directions
        if (direction.magnitude > 0.1f)
            direction.Normalize();

        return direction;
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
        }

        UpdateMoveAnimation();
    }

    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }

        UpdateMoveAnimation();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded())
            {
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                CharacterManager.Instance.Player.condition.UseStamina(10);
                animator.SetBool("Jump", true);
            }
            else if (canDoubleJump)
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                canDoubleJump = false;
                CharacterManager.Instance.Player.condition.UseStamina(10);
                animator.SetBool("Jump", true);
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            animator.SetBool("Jump", false);
        }
    }

    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }

    private void Move()
    {
        if (isLadder)
        {
            // Ladder movement remains vertical (unchanged)
            Vector3 dir = transform.up * curMovementInput.y;
            dir *= moveSpeed;
            Vector3 targetPosition = transform.position + dir * Time.fixedDeltaTime;
            rigidbody.MovePosition(targetPosition);
        }
        else
        {
            // Calculate movement speed
            float currentSpeed = isSprinting ? moveSpeed * 2f : moveSpeed;

            // Get movement direction relative to camera
            Vector3 dir = GetCameraRelativeMovementDirection(curMovementInput);

            // Only apply speed if there's actual direction input
            if (dir.magnitude > 0.1f)
            {
                // Apply movement speed
                dir *= currentSpeed;

                // Smoothly rotate player to face movement direction
                Quaternion targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Calculate target position
                Vector3 targetPosition = transform.position + dir * Time.fixedDeltaTime;

                // Apply movement
                rigidbody.MovePosition(targetPosition);
            }
            // If no movement input but still has velocity, apply a small amount of drag
            else if (rigidbody.velocity.magnitude > 0.1f && IsGrounded())
            {
                Vector3 horizontalVelocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
                rigidbody.velocity = new Vector3(
                    horizontalVelocity.x * 0.9f,
                    rigidbody.velocity.y,
                    horizontalVelocity.z * 0.9f
                );
            }
        }
    }


    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.01f;
        float rayDistance = 0.15f;

        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, rayDistance, groundLayerMask))
        {
            return true;
        }
        return false;
    }

    private void HandleAnimations()
    {
        bool isGrounded = IsGrounded();
        bool isFalling = !isGrounded && rigidbody.velocity.y < 0;

        if (isFalling)
        {
            animator.SetBool("FreeFall", true);
        }
        else if (isGrounded)
        {
            animator.SetBool("FreeFall", false);
            animator.SetBool("Jump", false);
            StartCoroutine(PlayLandAnimation());
        }
    }

    private IEnumerator PlayLandAnimation()
    {
        animator.SetTrigger("Grounded");
        yield return new WaitForSeconds(0.1f);
    }

    private void UpdateMoveAnimation()
    {
        float moveMagnitude = curMovementInput.magnitude * (isSprinting ? 8f : 2f);
        animator.SetFloat("Speed", moveMagnitude);
    }

    public void ApplyBoost(System.Action<float> applyAction, float baseValue, float amount, float duration, Sprite buffIcon)
    {
        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine);

        boostCoroutine = StartCoroutine(BoostCoroutine(applyAction, baseValue, amount, duration, buffIcon));
    }

    private IEnumerator DoubleJumpCoroutine(float duration, Sprite buffIcon)
    {
        canDoubleJump = true;
        buffUIManager.ShowBuff(buffIcon, duration);
        yield return new WaitForSeconds(duration);
        canDoubleJump = false;
    }

    private IEnumerator BoostCoroutine(System.Action<float> applyAction, float baseValue, float amount, float duration, Sprite buffIcon)
    {
        applyAction(baseValue + amount);
        buffUIManager.ShowBuff(buffIcon, duration);
        yield return new WaitForSeconds(duration);
        applyAction(baseValue);
    }
}