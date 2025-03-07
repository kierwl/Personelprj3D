using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;   // 발사할 총알 프리팹
    public Transform firePoint;       // 총알이 생성될 위치
    public float bulletSpeed = 10f;   // 총알 속도
    public float fireRate = 1f;       // 발사 속도 (초당 1발)

    private float nextFireTime = 0f;  // 다음 발사 가능 시간
    private Transform target;         // 목표 타겟

    private void Update()
    {
        if (target != null && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate; // 다음 발사 시간 설정
        }
    }

    /// <summary>
    /// 총알 발사
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.velocity = direction * bulletSpeed;
        }

        Debug.Log(" 총알 발사!");
    }

    /// <summary>
    /// 타겟 설정
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
