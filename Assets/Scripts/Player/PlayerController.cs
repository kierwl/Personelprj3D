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
    public bool canDoubleJump = false;  // ���� ���� ���� ���θ� �⺻������ false�� ����
    public LayerMask groundLayerMask;
    private Coroutine boostCoroutine;
    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;
    [Header("Other")]
    public BuffUIManager buffUIManager; // ���� UI �Ŵ��� ����
    private Vector2 mouseDelta;

    [HideInInspector]
    public bool canLook = true;
    public Action inventory;
    private Rigidbody rigidbody;
    private Animator animator;
    public bool isSprinting = false;
    public bool isLadder = false; // ��ٸ� ���¸� ��Ÿ���� �Ұ� �߰�

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

        // ������Ʈ ���� �� ���¹̳��� �Ҹ�
        if (isSprinting == true)
        {
            // ������Ʈ ������ ���� ���¹̳� �Ҹ�
            CharacterManager.Instance.Player.condition.UseStamina(15f * Time.deltaTime);
            Debug.Log("������Ʈ ��");
        }
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }
    private Coroutine doubleJumpCoroutine; // �ߺ� ������

    public void EnableDoubleJump(float duration, Sprite buffIcon)
    {
        Debug.Log("EnableDoubleJump called");
        if (doubleJumpCoroutine != null)
            StopCoroutine(doubleJumpCoroutine);  // ���� �ڷ�ƾ ����

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
            if (IsGrounded()) // ���� ���� �� ����
            {
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                CharacterManager.Instance.Player.condition.UseStamina(10);
                animator.SetBool("Jump", true);
            }
            else if (canDoubleJump) // ���߿��� �� �� �� ���� ����
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z); // ���� �ӵ� �ʱ�ȭ
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                canDoubleJump = false; // ���� ���� �� ��Ȱ��ȭ
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
            // ��ٸ� ������ ���� y�����θ� �̵�
            Vector3 dir = transform.up * curMovementInput.y;
            dir *= moveSpeed;
            Vector3 targetPosition = transform.position + dir * Time.fixedDeltaTime;
            rigidbody.MovePosition(targetPosition);
        }
        else
        {
            // �Ϲ� �̵�
            float currentSpeed = isSprinting ? moveSpeed * 2f : moveSpeed;
            Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
            dir *= currentSpeed;

            // y���� ������ �̵��� ���� (y�� �߷¿� ���� ������ �޵��� ����)
            Vector3 targetPosition = transform.position + dir * Time.fixedDeltaTime;

            // MovePosition�� ����Ͽ� ��ü�� �̵�
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
            animator.SetBool("Jump", false); // ���� �� Jump �ִϸ��̼� �Ұ��� false�� ����
            StartCoroutine(PlayLandAnimation());
        }
    }

    private IEnumerator PlayLandAnimation()
    {
        animator.SetTrigger("Grounded");
        yield return new WaitForSeconds(0.1f); // ���� ��� ���� �ð� ���� ����
    }

    private void UpdateMoveAnimation()
    {
        float moveMagnitude = curMovementInput.magnitude * (isSprinting ? 8f : 2f);
        animator.SetFloat("Speed", moveMagnitude);
    }

    public void ApplyBoost(System.Action<float> applyAction, float baseValue, float amount, float duration, Sprite buffIcon)
    {
        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine); // ���� �ڷ�ƾ ����

        boostCoroutine = StartCoroutine(BoostCoroutine(applyAction, baseValue, amount, duration, buffIcon));
    }

    private IEnumerator DoubleJumpCoroutine(float duration, Sprite buffIcon)
    {
        Debug.Log("DoubleJumpCoroutine started");
        canDoubleJump = true;  // ���� ���� Ȱ��ȭ
        buffUIManager.ShowBuff(buffIcon, duration); // UI ǥ��
        yield return new WaitForSeconds(duration);

        canDoubleJump = false;  // �ð��� ������ ��Ȱ��ȭ
        Debug.Log("DoubleJumpCoroutine ended");
    }

    private IEnumerator BoostCoroutine(System.Action<float> applyAction, float baseValue, float amount, float duration, Sprite buffIcon)
    {
        applyAction(baseValue + amount); // �ɷ�ġ ���� ����
        buffUIManager.ShowBuff(buffIcon, duration); // ���� UI ǥ��
        yield return new WaitForSeconds(duration);
        applyAction(baseValue); // ���� ������ ����
    }
}
