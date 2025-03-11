using UnityEngine;

public class FootSteps : MonoBehaviour
{
    public AudioClip[] footstepClips; // 발소리 클립들
    public AudioClip landClip;        // 착지 사운드 클립
    private AudioSource audioSource;  // 오디오 플레이어
    private Rigidbody _rigidbody;

    public float footstepThreshold = 1.0f; // 일정 속도 이상이면 발소리
    public float footstepRate = 0.5f;      // 발소리 간격
    private float footStepTime;            // 마지막 발소리 재생 시간

    private bool wasGrounded = true;       // 이전 프레임에서 땅에 붙어 있었는지

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        bool isGrounded = Mathf.Abs(_rigidbody.velocity.y) < 0.1f; // 현재 땅에 있는지 확인

        // 땅에 닿았을 때 (wasGrounded가 false -> true로 바뀌면 착지한 것)
        if (!wasGrounded && isGrounded)
        {
            OnLand();
        }

        // 땅에 있을 때 (OnFootstep 실행)
        if (isGrounded)
        {
            if (_rigidbody.velocity.magnitude > footstepThreshold) // 이동 속도 체크
            {
                if (Time.time - footStepTime > footstepRate) // 발소리 주기 체크
                {
                    footStepTime = Time.time;
                    OnFootstep();
                }
            }
        }

        wasGrounded = isGrounded; // 현재 상태를 저장 (다음 프레임 비교용)
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
