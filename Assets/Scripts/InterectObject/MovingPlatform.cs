using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private WaypointPath _waypointPath;

    public float moveSpeed = 2f;  // 이동 속도
    private int _targetWaypointIndex; // 다음 목표 위치 인덱스

    private Transform _previousWaypoint; // 이전 위치
    private Transform _targetWaypoint;   // 목표 위치

    private float timeToWaypoint = 0f; // 목표 위치까지의 시간
    private float elapsedTime = 0f;    // 경과 시간
    public LayerMask playerLayer; // 플레이어 감지 레이어
    public Vector3 detectionSize = new Vector3(2f, 0.5f, 2f); // 감지 범위


    private void Start()
    {
        TargetNextWaypoint();
    }
    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;
        float elapsedPercent = elapsedTime / timeToWaypoint;

        elapsedPercent = Mathf.SmoothStep(0, 1, elapsedPercent);
        transform.position = Vector3.Lerp(_previousWaypoint.position, _targetWaypoint.position, elapsedPercent);
        //transform.rotation = Vector3.Lerp(_previousWaypoint.rotation, _targetWaypoint.rotation, elapsedPercent); //회전을 주고 싶다면 주석해제

        if (elapsedPercent >= 1)
        {
            TargetNextWaypoint();
        }
    }
    private void TargetNextWaypoint()
    {
        _previousWaypoint = _waypointPath.GetWaypoint(_targetWaypointIndex);
        _targetWaypointIndex = _waypointPath.GetNextWaypointIndex(_targetWaypointIndex);
        _targetWaypoint = _waypointPath.GetWaypoint(_targetWaypointIndex);

        elapsedTime = 0f;

        float distance = Vector3.Distance(_previousWaypoint.position, _targetWaypoint.position);
        timeToWaypoint = distance / moveSpeed;
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
