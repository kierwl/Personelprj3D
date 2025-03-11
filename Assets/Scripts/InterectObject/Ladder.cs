using UnityEngine;

public class LadderObject : MonoBehaviour //, IInteractable
{
    // 플레이어가 사다리 영역 내에 있는지 여부
    private bool inside = false;
    public float speedUpDown = 3.2f; // 사다리 오르내리는 속도
    public float climbForce = 5f; // 사다리 오르내리는 속도


    // 플레이어 컨트롤러를 저장할 변수 (PlayerController가 있는 객체)
    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;

    // 플레이어가 Trigger에 진입하면 실행
    void OnTriggerEnter(Collider col)
    {
        // 플레이어 태그가 "Player"인 경우
        if (col.CompareTag("Player"))
        {
            playerController = col.GetComponent<PlayerController>();
            playerRigidbody = col.GetComponent<Rigidbody>();
            if (playerController != null && playerRigidbody != null)
            {
                // 플레이어의 기존 이동을 비활성화하여 사다리 오르내리기에 집중하도록 함
                playerController.isLadder = true; // 사다리 상태 활성화
                inside = true;
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.useGravity = false;
                playerTransform = col.transform;
            }
        }
    }

    // 플레이어가 Trigger 영역을 벗어나면 실행
    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (playerController != null)
            {
                // 플레이어 이동 다시 활성화
                playerController.isLadder = false; // 사다리 상태 비활성화
                playerRigidbody.useGravity = true;
            }
            inside = false;
            playerTransform = null;
            playerRigidbody.velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (inside && playerRigidbody != null)
        {
            if (Input.GetKey("w"))
            {
                playerRigidbody.AddForce(Vector3.up * climbForce, ForceMode.Acceleration);
            }
            else if (Input.GetKey("s"))
            {
                playerRigidbody.AddForce(Vector3.down * climbForce, ForceMode.Acceleration);
            }
            else
            {
                // 키를 떼면 즉시 속도를 0으로 설정 (추가적인 힘을 가하지 않음)
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            }
        }
    }
}
