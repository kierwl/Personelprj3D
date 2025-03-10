using UnityEngine;
using System.Collections;
public class Shoot : MonoBehaviour
{
    public TurretAim turretAim;
    public BulletPoolManager bulletPoolManager;  // 총알 풀 매니저
    public Transform firePoint;  // 총알 발사 위치
    public float bulletSpeed = 20f;  // 총알 속도
    public float fireRate = 5f;  // 발사 속도 (초당 1발)
    public bool isShooting = false;  // 발사 여부   
    private float nextFireTime = 1f;  // 다음 발사 가능 시간
    private Transform target;  // 목표 타겟

    private void Update()
    {
        if (target != null && Time.time >= nextFireTime)
        {
            if (isShooting)
            {
                Fire();
                Debug.Log("총알 발사!");
                nextFireTime = Time.time + 1f / fireRate; // 다음 발사 시간 설정
            }
        }
        else
        {
            // 타겟이 없거나 발사 대기 시간이 아닌 경우
            turretAim.IsIdle = true;
            isShooting = false;
        }
    }

    /// <summary>
    /// 총알 발사
    /// </summary>
    public void Fire()
    {
        if (bulletPoolManager == null || firePoint == null) return;

        // 총알 풀에서 총알을 꺼냄
        GameObject bullet = bulletPoolManager.GetBullet(firePoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.velocity = direction * bulletSpeed;
        }

        // 충돌 검사
        StartCoroutine(CheckBulletCollision(bullet));

        Debug.Log(" 총알 발사!");
    }

    /// <summary>
    /// 총알이 플레이어와 충돌하는지 체크
    /// </summary>
    private IEnumerator CheckBulletCollision(GameObject bullet)
    {
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        float bulletLifeTime = 0f;
        float maxLifeTime = 3f; // 최대 생존 시간

        while (bullet.activeSelf)
        {
            bulletLifeTime += Time.deltaTime;

            RaycastHit hit;
            if (Physics.Raycast(bullet.transform.position, rb.velocity.normalized, out hit, bulletSpeed * Time.deltaTime))
            {
                // 플레이어와 충돌한 경우
                if (hit.collider.CompareTag("Player"))
                {
                    // 플레이어와 충돌 시 총알 비활성화 및 위치 초기화
                    bulletPoolManager.ReturnBulletToPool(bullet);
                    CharacterManager.Instance.Player.controller.GetComponent<IDamageable>().TakePhysicalDamage(5);

                    // 넉백 효과 추가
                    Rigidbody playerRb = hit.collider.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 knockbackDirection = (hit.collider.transform.position - bullet.transform.position).normalized;
                        playerRb.AddForce(knockbackDirection * 10f, ForceMode.VelocityChange); // 넉백 힘 적용
                    }

                    Debug.Log(" 플레이어와 충돌, 총알 리턴!");
                    break;
                }
            }

            // 총알이 3초 이상 살아있다면 충돌 판정
            if (bulletLifeTime >= maxLifeTime)
            {
                bulletPoolManager.ReturnBulletToPool(bullet);
                Debug.Log(" 총알이 3초 이상 살아있어 리턴!");
                break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 타겟 설정
    /// </summary>
    public void SetTarget(Vector3 targetPosition)
    {
        turretAim.AimPosition = targetPosition;
        turretAim.IsIdle = false; // 타겟이 설정되면 대기 상태를 해제

        // 기존 target이 존재하면 삭제
        if (target != null)
        {
            Destroy(target.gameObject);
        }

        // target 변수를 설정
        GameObject targetObject = new GameObject("Target");
        targetObject.transform.position = targetPosition;
        target = targetObject.transform;
    }
}
