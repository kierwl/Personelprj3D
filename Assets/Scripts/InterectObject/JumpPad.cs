using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 30f;  // 점프 강도 설정

    // 캐릭터가 점프대에 닿았을 때
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체가 "Player" 태그를 가지고 있는지 확인
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 기존 Y 속도를 초기화하여 점프대에서 적용되는 힘이 제대로 전달되도록 함
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

                // 위쪽으로 순간적인 힘을 추가 (ForceMode.Impulse)
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }
}
