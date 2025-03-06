using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 30f;  // ���� ���� ����

    // ĳ���Ͱ� �����뿡 ����� ��
    private void OnCollisionEnter(Collision collision)
    {
        // �浹�� ��ü�� "Player" �±׸� ������ �ִ��� Ȯ��
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ���� Y �ӵ��� �ʱ�ȭ�Ͽ� �����뿡�� ����Ǵ� ���� ����� ���޵ǵ��� ��
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

                // �������� �������� ���� �߰� (ForceMode.Impulse)
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }
}
