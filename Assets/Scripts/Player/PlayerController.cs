using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 curMovementInput;
    public float jumpPower;
    public bool canDoubleJump = false;  // 더블 점프 가능 여부를 기본적으로 false로 설정
    public LayerMask groundLayerMask;
    private Coroutine boostCoroutine;
    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;
    [Header("Other")]
    public BuffUIManager buffUIManager; // 버프 UI 매니저 연결
    private Vector2 mouseDelta;

    [HideInInspector]
    public bool canLook = true;
    public Action inventory;
    private Rigidbody rigidbody;
    private Animator animator;
    public bool isSprinting = false;
    public bool isLadder = false; // 사다리 상태를 나타내는 불값 추가

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

        // 스프린트 중일 때 스태미나를 소모
        if (isSprinting == true)
        {
            // 스프린트 상태일 때만 스태미나 소모
            CharacterManager.Instance.Player.condition.UseStamina(15f * Time.deltaTime);
            Debug.Log("스프린트 중");
        }
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }
    private Coroutine doubleJumpCoroutine; // 중복 방지용

    public void EnableDoubleJump(float duration, Sprite buffIcon)
    {
        Debug.Log("EnableDoubleJump called");
        if (doubleJumpCoroutine != null)
            StopCoroutine(doubleJumpCoroutine);  // 기존 코루틴 중지

        doubleJumpCoroutine = StartCoroutine(DoubleJumpCoroutine(duration, buffIcon));
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
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded()) // 땅에 있을 때 점프
            {
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                CharacterManager.Instance.Player.condition.UseStamina(10);
                animator.SetBool("Jump", true);
            }
            else if (canDoubleJump) // 공중에서 한 번 더 점프 가능
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z); // 현재 속도 초기화
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                canDoubleJump = false; // 더블 점프 후 비활성화
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
            // 사다리 상태일 때는 y축으로만 이동
            Vector3 dir = transform.up * curMovementInput.y;
            dir *= moveSpeed;
            Vector3 targetPosition = transform.position + dir * Time.fixedDeltaTime;
            rigidbody.MovePosition(targetPosition);
        }
        else
        {
            // 일반 이동
            float currentSpeed = isSprinting ? moveSpeed * 2f : moveSpeed;
            Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
            dir *= currentSpeed;

            // y값을 제외한 이동만 적용 (y는 중력에 의해 영향을 받도록 유지)
            Vector3 targetPosition = transform.position + dir * Time.fixedDeltaTime;

            // MovePosition을 사용하여 물체를 이동
            rigidbody.MovePosition(targetPosition);
        }
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
            animator.SetBool("Jump", false); // 착지 시 Jump 애니메이션 불값을 false로 설정
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
        float moveMagnitude = curMovementInput.magnitude * (isSprinting ? 8f : 2f);
        animator.SetFloat("Speed", moveMagnitude);
    }

    public void ApplyBoost(System.Action<float> applyAction, float baseValue, float amount, float duration, Sprite buffIcon)
    {
        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine); // 기존 코루틴 중지

        boostCoroutine = StartCoroutine(BoostCoroutine(applyAction, baseValue, amount, duration, buffIcon));
    }

    private IEnumerator DoubleJumpCoroutine(float duration, Sprite buffIcon)
    {
        Debug.Log("DoubleJumpCoroutine started");
        canDoubleJump = true;  // 더블 점프 활성화
        buffUIManager.ShowBuff(buffIcon, duration); // UI 표시
        yield return new WaitForSeconds(duration);

        canDoubleJump = false;  // 시간이 지나면 비활성화
        Debug.Log("DoubleJumpCoroutine ended");
    }

    private IEnumerator BoostCoroutine(System.Action<float> applyAction, float baseValue, float amount, float duration, Sprite buffIcon)
    {
        applyAction(baseValue + amount); // 능력치 증가 적용
        buffUIManager.ShowBuff(buffIcon, duration); // 버프 UI 표시
        yield return new WaitForSeconds(duration);
        applyAction(baseValue); // 원래 값으로 복구
    }
}
