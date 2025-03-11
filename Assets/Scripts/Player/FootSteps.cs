using UnityEngine;

public class FootSteps : MonoBehaviour
{
    public AudioClip[] footstepClips; // �߼Ҹ� Ŭ����
    public AudioClip landClip;        // ���� ���� Ŭ��
    private AudioSource audioSource;  // ����� �÷��̾�
    private Rigidbody _rigidbody;

    public float footstepThreshold = 1.0f; // ���� �ӵ� �̻��̸� �߼Ҹ�
    public float footstepRate = 0.5f;      // �߼Ҹ� ����
    private float footStepTime;            // ������ �߼Ҹ� ��� �ð�

    private bool wasGrounded = true;       // ���� �����ӿ��� ���� �پ� �־�����

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        bool isGrounded = Mathf.Abs(_rigidbody.velocity.y) < 0.1f; // ���� ���� �ִ��� Ȯ��

        // ���� ����� �� (wasGrounded�� false -> true�� �ٲ�� ������ ��)
        if (!wasGrounded && isGrounded)
        {
            OnLand();
        }

        // ���� ���� �� (OnFootstep ����)
        if (isGrounded)
        {
            if (_rigidbody.velocity.magnitude > footstepThreshold) // �̵� �ӵ� üũ
            {
                if (Time.time - footStepTime > footstepRate) // �߼Ҹ� �ֱ� üũ
                {
                    footStepTime = Time.time;
                    OnFootstep();
                }
            }
        }

        wasGrounded = isGrounded; // ���� ���¸� ���� (���� ������ �񱳿�)
    }

    private void OnFootstep()
    {
        if (footstepClips.Length > 0 && audioSource != null)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnLand()
    {
        if (landClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(landClip);
        }
    }
}
