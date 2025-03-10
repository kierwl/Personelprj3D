using UnityEngine;
using System.Collections;
public class Shoot : MonoBehaviour
{
    public TurretAim turretAim;
    public BulletPoolManager bulletPoolManager;  // �Ѿ� Ǯ �Ŵ���
    public Transform firePoint;  // �Ѿ� �߻� ��ġ
    public float bulletSpeed = 20f;  // �Ѿ� �ӵ�
    public float fireRate = 5f;  // �߻� �ӵ� (�ʴ� 1��)
    public bool isShooting = false;  // �߻� ����   
    private float nextFireTime = 1f;  // ���� �߻� ���� �ð�
    private Transform target;  // ��ǥ Ÿ��

    private void Update()
    {
        if (target != null && Time.time >= nextFireTime)
        {
            if (isShooting)
            {
                Fire();
                Debug.Log("�Ѿ� �߻�!");
                nextFireTime = Time.time + 1f / fireRate; // ���� �߻� �ð� ����
            }
        }
        else
        {
            // Ÿ���� ���ų� �߻� ��� �ð��� �ƴ� ���
            turretAim.IsIdle = true;
            isShooting = false;
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
        float bulletLifeTime = 0f;
        float maxLifeTime = 3f; // �ִ� ���� �ð�

        while (bullet.activeSelf)
        {
            bulletLifeTime += Time.deltaTime;

            RaycastHit hit;
            if (Physics.Raycast(bullet.transform.position, rb.velocity.normalized, out hit, bulletSpeed * Time.deltaTime))
            {
                // �÷��̾�� �浹�� ���
                if (hit.collider.CompareTag("Player"))
                {
                    // �÷��̾�� �浹 �� �Ѿ� ��Ȱ��ȭ �� ��ġ �ʱ�ȭ
                    bulletPoolManager.ReturnBulletToPool(bullet);
                    CharacterManager.Instance.Player.controller.GetComponent<IDamageable>().TakePhysicalDamage(5);

                    // �˹� ȿ�� �߰�
                    Rigidbody playerRb = hit.collider.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 knockbackDirection = (hit.collider.transform.position - bullet.transform.position).normalized;
                        playerRb.AddForce(knockbackDirection * 10f, ForceMode.VelocityChange); // �˹� �� ����
                    }

                    Debug.Log(" �÷��̾�� �浹, �Ѿ� ����!");
                    break;
                }
            }

            // �Ѿ��� 3�� �̻� ����ִٸ� �浹 ����
            if (bulletLifeTime >= maxLifeTime)
            {
                bulletPoolManager.ReturnBulletToPool(bullet);
                Debug.Log(" �Ѿ��� 3�� �̻� ����־� ����!");
                break;
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

        // ���� target�� �����ϸ� ����
        if (target != null)
        {
            Destroy(target.gameObject);
        }

        // target ������ ����
        GameObject targetObject = new GameObject("Target");
        targetObject.transform.position = targetPosition;
        target = targetObject.transform;
    }
}
