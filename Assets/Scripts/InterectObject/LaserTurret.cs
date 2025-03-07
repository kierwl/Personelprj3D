using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    public Transform laserOrigin;  // ������ �߻� ����
    public Transform firePoint;  // �������� �߻�� ����
    public GameObject laserPrefab;  // �߻��� ������ ������
    public float detectionRange = 10f;  // ���� ����
    public float attackInterval = 1.5f;  // ���� ���� (��)
    public float rotationSpeed = 5f;  // ȸ�� �ӵ�
    public LayerMask playerLayer;  // �÷��̾� ���̾�
    public AudioClip laserSound;  // ������ �߻� �Ҹ�

    private Transform targetPlayer;  // ������ �÷��̾�
    private bool isShooting = false;  // ���� ���� ������ ����

    private void Update()
    {
        CheckForPlayer();

        // �÷��̾ �����Ǹ� ���� ȸ��
        if (targetPlayer != null)
        {
            RotateTowardsTarget();
        }
    }

    // �÷��̾� ����
    private void CheckForPlayer()
    {
        RaycastHit hit;
        Vector3 direction = transform.forward;  // �⺻������ ���� ���� ����

        if (Physics.Raycast(laserOrigin.position, direction, out hit, detectionRange, playerLayer))
        {
            targetPlayer = hit.transform;

            // ���� ����
            if (!isShooting)
            {
                isShooting = true;
                InvokeRepeating("FireLaser", 0f, attackInterval);
            }
        }
        else
        {
            targetPlayer = null;

            if (isShooting)
            {
                isShooting = false;
                CancelInvoke("FireLaser");  // ���� �ߴ�
            }
        }
    }

    // �÷��̾� �������� �ε巴�� ȸ��
    private void RotateTowardsTarget()
    {
        if (targetPlayer == null) return;

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // ������ �߻�
    private void FireLaser()
    {
        if (laserPrefab != null && targetPlayer != null)
        {
            Instantiate(laserPrefab, firePoint.position, firePoint.rotation);  // ������ �߻�
            if (laserSound != null)
            {
                AudioSource.PlayClipAtPoint(laserSound, firePoint.position);  // ������ �Ҹ� ���
            }
        }
    }
}
