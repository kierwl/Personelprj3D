using UnityEngine;
using System.Collections;
public class Shoot : MonoBehaviour
{
    public TurretAim turretAim;
    public BulletPoolManager bulletPoolManager;  // �Ѿ� Ǯ �Ŵ���
    public Transform firePoint;  // �Ѿ� �߻� ��ġ
    public float bulletSpeed = 10f;  // �Ѿ� �ӵ�
    public float fireRate = 1f;  // �߻� �ӵ� (�ʴ� 1��)

    private float nextFireTime = 0f;  // ���� �߻� ���� �ð�
    private Transform target;  // ��ǥ Ÿ��

    private void Update()
    {
        if (target != null && Time.time >= nextFireTime)
        {
            Fire();
            Debug.Log("�Ѿ� �߻�!");
            nextFireTime = Time.time + 1f / fireRate; // ���� �߻� �ð� ����
        }
        else
        {
            // Ÿ���� ���ų� �߻� ��� �ð��� �ƴ� ���
            turretAim.IsIdle = true;
        }
    }

    /// <summary>
    /// �Ѿ� �߻�
    /// </summary>
    public void Fire()
    {
        if (bulletPoolManager == null || firePoint == null) return;

        // �Ѿ� Ǯ���� �Ѿ��� ����
        GameObject bullet = bulletPoolManager.GetBullet(firePoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.velocity = direction * bulletSpeed;
        }

        // �浹 �˻�
        StartCoroutine(CheckBulletCollision(bullet));

        Debug.Log(" �Ѿ� �߻�!");
    }

    /// <summary>
    /// �Ѿ��� �÷��̾�� �浹�ϴ��� üũ
    /// </summary>
    private IEnumerator CheckBulletCollision(GameObject bullet)
    {
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        while (bullet.activeSelf)
        {
            RaycastHit hit;
            if (Physics.Raycast(bullet.transform.position, rb.velocity.normalized, out hit, bulletSpeed * Time.deltaTime))
            {
                // �÷��̾�� �浹�� ���
                if (hit.collider.CompareTag("Player"))
                {
                    // �÷��̾�� �浹 �� �Ѿ� ��Ȱ��ȭ �� ��ġ �ʱ�ȭ
                    bulletPoolManager.ReturnBulletToPool(bullet);
                    CharacterManager.Instance.Player.controller.GetComponent<IDamageable>().TakePhysicalDamage(5);
                    Debug.Log(" �÷��̾�� �浹, �Ѿ� ����!");
                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Ÿ�� ����
    /// </summary>
    public void SetTarget(Vector3 targetPosition)
    {
        turretAim.AimPosition = targetPosition;
        turretAim.IsIdle = false; // Ÿ���� �����Ǹ� ��� ���¸� ����
    }
}
