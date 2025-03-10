using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public LayerMask playerLayer; // 플레이어 감지 레이어
    public Vector3 detectionSize = new Vector3(2f, 0.5f, 2f); // 감지 범위


    public float rotationSpeed = 100f; // 회전 속도

    // 각 축의 회전 여부를 결정하는 bool 값
    public bool rotateX = false;
    public bool rotateY = true;
    public bool rotateZ = false;

    // 회전 방향을 결정하는 bool 값 (true면 시계 방향, false면 반시계 방향)
    public bool isClockwise = true;

    void Update()
    {
        float direction = isClockwise ? 1f : -1f; // 방향 설정
        Vector3 rotationAxis = new Vector3(
            rotateX ? direction : 0f,
            rotateY ? direction : 0f,
            rotateZ ? direction : 0f
        );

        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 레이어에 속한 객체만 자식으로 설정
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어 레이어에 속한 객체만 부모에서 해제
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            other.transform.SetParent(null);
        }
    }
}
