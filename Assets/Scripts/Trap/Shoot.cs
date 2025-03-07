using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;   // �߻��� �Ѿ� ������
    public Transform firePoint;       // �Ѿ��� ������ ��ġ
    public float bulletSpeed = 10f;   // �Ѿ� �ӵ�
    public float fireRate = 1f;       // �߻� �ӵ� (�ʴ� 1��)

    private float nextFireTime = 0f;  // ���� �߻� ���� �ð�
    private Transform target;         // ��ǥ Ÿ��

    private void Update()
    {
        if (target != null && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate; // ���� �߻� �ð� ����
        }
    }

    /// <summary>
    /// �Ѿ� �߻�
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // �Ѿ� ����
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.velocity = direction * bulletSpeed;
        }

        Debug.Log(" �Ѿ� �߻�!");
    }

    /// <summary>
    /// Ÿ�� ����
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
