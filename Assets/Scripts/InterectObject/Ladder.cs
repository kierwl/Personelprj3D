using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float climbSpeed = 5f; // 사다리 이동 속도
    private bool isClimbing = false; // 현재 사다리 타는 중인지 체크
    private Rigidbody playerRb; // 플레이어의 Rigidbody

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isClimbing = true;
            playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                playerRb.useGravity = false; // 사다리 영역에서 중력 해제
                playerRb.velocity = Vector3.zero; // 기존 속도 초기화
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isClimbing = false;
            if (playerRb != null)
            {
                playerRb.useGravity = true; // 사다리를 벗어나면 중력 다시 적용
            }
        }
    }

    private void Update()
    {
        if (isClimbing && playerRb != null)
        {
            float verticalInput = Input.GetAxis("Vertical"); // W / S 키 또는 ↑ / ↓ 키 입력
            playerRb.velocity = new Vector3(0, verticalInput * climbSpeed, 0);
        }
    }
}
