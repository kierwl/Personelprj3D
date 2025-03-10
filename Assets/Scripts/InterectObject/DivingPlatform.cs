using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingPlatform : MonoBehaviour
{
    public Vector3 targetPosition; // ��ǥ ���� (�÷��̾ �߻�� ��ġ)
    public float launchForce = 15f; // �߻� ��
    public float height = 5f; // ������ ����
    public float duration = 1.5f; // �߻� �ð�
    public float targetYOffset = 5f; // ��ǥ ������ Y�� ������

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �÷��̾��� Rigidbody�� �����ͼ� ���� ������ �غ�
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // 3�� ��� �� �÷��̾� �߻�
                StartCoroutine(LaunchPlayerAfterDelay(playerRb, 3f));
            }
        }
    }

    private IEnumerator LaunchPlayerAfterDelay(Rigidbody playerRb, float delay)
    {
        yield return new WaitForSeconds(delay); // 3�� ���

        Vector3 direction = (targetPosition - playerRb.transform.position).normalized; // ��ǥ ���� ���
        direction.y = 1f; // Y�� �������� �߰����� ���� �༭ �������� ����� ���� ���� ���⿡ ������ ��

        // ��ǥ �������� ���ư����� ���� ���� (X, Y, Z ��ο� ���� ��)
        playerRb.velocity = Vector3.zero; // ���� �ӵ� �ʱ�ȭ
        playerRb.AddForce(direction * launchForce, ForceMode.VelocityChange); // �߻� ���� �־ ������
    }
}

