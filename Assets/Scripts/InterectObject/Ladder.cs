using UnityEngine;

public class LadderObject : MonoBehaviour //, IInteractable
{
    // �÷��̾ ��ٸ� ���� ���� �ִ��� ����
    private bool inside = false;
    public float speedUpDown = 3.2f; // ��ٸ� ���������� �ӵ�
    public float climbForce = 5f; // ��ٸ� ���������� �ӵ�


    // �÷��̾� ��Ʈ�ѷ��� ������ ���� (PlayerController�� �ִ� ��ü)
    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;

    // �÷��̾ Trigger�� �����ϸ� ����
    void OnTriggerEnter(Collider col)
    {
        // �÷��̾� �±װ� "Player"�� ���
        if (col.CompareTag("Player"))
        {
            playerController = col.GetComponent<PlayerController>();
            playerRigidbody = col.GetComponent<Rigidbody>();
            if (playerController != null && playerRigidbody != null)
            {
                // �÷��̾��� ���� �̵��� ��Ȱ��ȭ�Ͽ� ��ٸ� ���������⿡ �����ϵ��� ��
                playerController.isLadder = true; // ��ٸ� ���� Ȱ��ȭ
                inside = true;
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.useGravity = false;
                playerTransform = col.transform;
            }
        }
    }

    // �÷��̾ Trigger ������ ����� ����
    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (playerController != null)
            {
                // �÷��̾� �̵� �ٽ� Ȱ��ȭ
                playerController.isLadder = false; // ��ٸ� ���� ��Ȱ��ȭ
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
                // Ű�� ���� ��� �ӵ��� 0���� ���� (�߰����� ���� ������ ����)
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            }
        }
    }
}
