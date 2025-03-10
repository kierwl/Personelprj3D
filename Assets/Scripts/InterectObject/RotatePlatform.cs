using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public LayerMask playerLayer; // �÷��̾� ���� ���̾�
    public Vector3 detectionSize = new Vector3(2f, 0.5f, 2f); // ���� ����


    public float rotationSpeed = 100f; // ȸ�� �ӵ�

    // �� ���� ȸ�� ���θ� �����ϴ� bool ��
    public bool rotateX = false;
    public bool rotateY = true;
    public bool rotateZ = false;

    // ȸ�� ������ �����ϴ� bool �� (true�� �ð� ����, false�� �ݽð� ����)
    public bool isClockwise = true;

    void Update()
    {
        float direction = isClockwise ? 1f : -1f; // ���� ����
        Vector3 rotationAxis = new Vector3(
            rotateX ? direction : 0f,
            rotateY ? direction : 0f,
            rotateZ ? direction : 0f
        );

        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾� ���̾ ���� ��ü�� �ڽ����� ����
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �÷��̾� ���̾ ���� ��ü�� �θ𿡼� ����
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            other.transform.SetParent(null);
        }
    }
}
