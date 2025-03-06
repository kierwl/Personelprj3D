using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float climbSpeed = 5f; // ��ٸ� �̵� �ӵ�
    private bool isClimbing = false; // ���� ��ٸ� Ÿ�� ������ üũ
    private Rigidbody playerRb; // �÷��̾��� Rigidbody

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isClimbing = true;
            playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                playerRb.useGravity = false; // ��ٸ� �������� �߷� ����
                playerRb.velocity = Vector3.zero; // ���� �ӵ� �ʱ�ȭ
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
                playerRb.useGravity = true; // ��ٸ��� ����� �߷� �ٽ� ����
            }
        }
    }

    private void Update()
    {
        if (isClimbing && playerRb != null)
        {
            float verticalInput = Input.GetAxis("Vertical"); // W / S Ű �Ǵ� �� / �� Ű �Է�
            playerRb.velocity = new Vector3(0, verticalInput * climbSpeed, 0);
        }
    }
}
