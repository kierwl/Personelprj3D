using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private WaypointPath _waypointPath;

    public float moveSpeed = 2f;  // �̵� �ӵ�
    private int _targetWaypointIndex; // ���� ��ǥ ��ġ �ε���

    private Transform _previousWaypoint; // ���� ��ġ
    private Transform _targetWaypoint;   // ��ǥ ��ġ

    private float timeToWaypoint = 0f; // ��ǥ ��ġ������ �ð�
    private float elapsedTime = 0f;    // ��� �ð�
    public LayerMask playerLayer; // �÷��̾� ���� ���̾�
    public Vector3 detectionSize = new Vector3(2f, 0.5f, 2f); // ���� ����


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
        //transform.rotation = Vector3.Lerp(_previousWaypoint.rotation, _targetWaypoint.rotation, elapsedPercent); //ȸ���� �ְ� �ʹٸ� �ּ�����

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
        // �÷��̾� ���̾ ���� ��ü�� �ڽ����� ����
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �÷��̾� ���̾ ���� ��ü�� �θ𿡼� ����
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            other.transform.SetParent(null);
        }
    }
}
