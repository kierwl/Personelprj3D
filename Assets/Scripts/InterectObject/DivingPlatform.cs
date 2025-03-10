using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingPlatform : MonoBehaviour
{
    public Vector3 targetPosition; // 목표 지점 (플레이어가 발사될 위치)
    public float launchForce = 15f; // 발사 힘
    public float height = 5f; // 포물선 높이
    public float duration = 1.5f; // 발사 시간
    public float targetYOffset = 5f; // 목표 지점의 Y축 오프셋

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어의 Rigidbody를 가져와서 힘을 적용할 준비
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // 3초 대기 후 플레이어 발사
                StartCoroutine(LaunchPlayerAfterDelay(playerRb, 3f));
            }
        }
    }

    private IEnumerator LaunchPlayerAfterDelay(Rigidbody playerRb, float delay)
    {
        yield return new WaitForSeconds(delay); // 3초 대기

        Vector3 direction = (targetPosition - playerRb.transform.position).normalized; // 목표 방향 계산
        direction.y = 1f; // Y축 방향으로 추가적인 힘을 줘서 포물선을 만들기 위해 수직 방향에 영향을 줌

        // 목표 지점까지 날아가도록 힘을 적용 (X, Y, Z 모두에 대한 힘)
        playerRb.velocity = Vector3.zero; // 기존 속도 초기화
        playerRb.AddForce(direction * launchForce, ForceMode.VelocityChange); // 발사 힘을 주어서 날리기
    }
}

