using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    public Transform laserOrigin;  // 레이저 발사 지점
    public Transform firePoint;  // 레이저가 발사될 지점
    public GameObject laserPrefab;  // 발사할 레이저 프리팹
    public float detectionRange = 10f;  // 감지 범위
    public float attackInterval = 1.5f;  // 공격 간격 (초)
    public float rotationSpeed = 5f;  // 회전 속도
    public LayerMask playerLayer;  // 플레이어 레이어
    public AudioClip laserSound;  // 레이저 발사 소리

    private Transform targetPlayer;  // 추적할 플레이어
    private bool isShooting = false;  // 현재 공격 중인지 여부

    private void Update()
    {
        CheckForPlayer();

        // 플레이어가 감지되면 방향 회전
        if (targetPlayer != null)
        {
            RotateTowardsTarget();
        }
    }

    // 플레이어 감지
    private void CheckForPlayer()
    {
        RaycastHit hit;
        Vector3 direction = transform.forward;  // 기본적으로 정면 방향 감지

        if (Physics.Raycast(laserOrigin.position, direction, out hit, detectionRange, playerLayer))
        {
            targetPlayer = hit.transform;

            // 공격 시작
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
                CancelInvoke("FireLaser");  // 공격 중단
            }
        }
    }

    // 플레이어 방향으로 부드럽게 회전
    private void RotateTowardsTarget()
    {
        if (targetPlayer == null) return;

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // 레이저 발사
    private void FireLaser()
    {
        if (laserPrefab != null && targetPlayer != null)
        {
            Instantiate(laserPrefab, firePoint.position, firePoint.rotation);  // 레이저 발사
            if (laserSound != null)
            {
                AudioSource.PlayClipAtPoint(laserSound, firePoint.position);  // 레이저 소리 재생
            }
        }
    }
}
