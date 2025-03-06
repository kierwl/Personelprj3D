using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 curMovementInput;
    public float jumpPower;
    public LayerMask groundLayerMask;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;

    private Vector2 mouseDelta;

    [HideInInspector]
    public bool canLook = true;
    public Action inventory;
    private Rigidbody rigidbody;
    private Animator animator;
    private bool isSprinting = false;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleAnimations();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
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
        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            animator.SetBool("Jump",true);
        }
        else
        {
            animator.SetBool("Jump",false);
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
        float currentSpeed = isSprinting ? moveSpeed * 2f : moveSpeed;
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= currentSpeed;
        dir.y = rigidbody.velocity.y;

        rigidbody.velocity = dir;
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.01f;
        float rayDistance = 0.1f;

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
        else if (isGrounded && animator.GetBool("FreeFall"))
        {
            animator.SetBool("FreeFall", false);
            StartCoroutine(PlayLandAnimation());
        }
    }

    private IEnumerator PlayLandAnimation()
    {
        animator.SetTrigger("Grounded");
        yield return new WaitForSeconds(0.1f); // 착지 모션 유지 시간 조정 가능
    }

    private void UpdateMoveAnimation()
    {
        float moveMagnitude = curMovementInput.magnitude * (isSprinting ? 6f : 2f);
        animator.SetFloat("Speed", moveMagnitude);
    }
}
