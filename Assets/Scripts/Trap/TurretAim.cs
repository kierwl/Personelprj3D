using UnityEngine;

public class TurretAim : MonoBehaviour
{
    [Header("Rotations")]
    [Tooltip("포탑의 방위각 회전의 Transform입니다.")]
    [SerializeField] private Transform turretBase = null;

    [Tooltip("포탑의 고각 회전의 Transform입니다.")]
    [SerializeField] private Transform barrels = null;

    [Header("Elevation")]
    [Tooltip("포탑의 총신이 위아래로 움직이는 속도입니다.")]
    public float ElevationSpeed = 30f;

    [Tooltip("포탑의 총신이 조준할 수 있는 가장 높은 고각입니다.")]
    public float MaxElevation = 60f;

    [Tooltip("포탑의 총신이 조준할 수 있는 가장 낮은 하각입니다.")]
    public float MaxDepression = 5f;

    [Header("Traverse")]
    [Tooltip("포탑이 좌우로 회전할 수 있는 속도입니다.")]
    public float TraverseSpeed = 60f;

    [Tooltip("true일 경우, 포탑은 주어진 한계 내에서만 수평으로 회전할 수 있습니다.")]
    [SerializeField] private bool hasLimitedTraverse = false;
    [Range(0, 179)] public float LeftLimit = 120f;
    [Range(0, 179)] public float RightLimit = 120f;

    [Header("Behavior")]
    [Tooltip("대기 중일 때, 포탑은 아무것도 조준하지 않고 단순히 앞으로 향합니다.")]
    public bool IsIdle = false;

    [Tooltip("대기 중이 아닐 때 포탑이 조준할 위치입니다. 포탑이 적극적으로 조준할 위치를 설정하십시오.")]
    public Vector3 AimPosition = Vector3.zero;

    [Tooltip("포탑이 목표물에 이 정도 각도 이내에 있을 때, 조준된 것으로 간주됩니다.")]
    [SerializeField] private float aimedThreshold = 5f;
    private float limitedTraverseAngle = 0f;

    [Tooltip("레이캐스트를 위한 레이어 마스크")]
    public LayerMask targetLayer;

    [Tooltip("장애물을 감지할 레이어")]
    public LayerMask obstacleLayer;

    [Tooltip("포탑이 목표를 지속적으로 추적할지 여부")]
    public bool trackTarget = true;
    public float detectionRange = 10f;
    private Transform target;

    [Header("Debug")]
    public bool DrawDebugRay = true;
    public bool DrawDebugArcs = false;

    private float angleToTarget = 0f;
    private float elevation = 0f;

    private bool hasBarrels = false;

    private bool isAimed = false;
    private bool isBaseAtRest = false;
    private bool isBarrelAtRest = false;

    private Shoot shoot;

    /// <summary>
    /// 포탑이 수평 축에서 자유롭게 회전할 수 없을 때 true입니다.
    /// </summary>
    public bool HasLimitedTraverse { get { return hasLimitedTraverse; } }

    /// <summary>
    /// 포탑이 대기 중이고 휴식 위치에 있을 때 true입니다.
    /// </summary>
    public bool IsTurretAtRest { get { return isBarrelAtRest && isBaseAtRest; } }

    /// <summary>
    /// 포탑이 주어진 <see cref="AimPosition"/>을 조준하고 있을 때 true입니다. 포탑이 대기 중일 때는 이 값이 절대 true가 아닙니다.
    /// </summary>
    public bool IsAimed { get { return isAimed; } }

    /// <summary>
    /// 주어진 <see cref="AimPosition"/>까지의 각도입니다. 포탑이 대기 중일 때는 각도가 999로 보고됩니다.
    /// </summary>
    public float AngleToTarget { get { return IsIdle ? 999f : angleToTarget; } }

    private void Awake()
    {
        hasBarrels = barrels != null;
        if (turretBase == null)
            Debug.LogError(name + ": TurretAim에는 할당된 TurretBase가 필요합니다!");
    }

    private void Start()
    {
        shoot = GetComponent<Shoot>();
    }

    private void Update()
    {
        DetectTarget();
        if (IsIdle)
        {
            if (!IsTurretAtRest)
                RotateTurretToIdle();
            isAimed = false;
            shoot.isShooting = false;
        }
        else
        {
            RotateBaseToFaceTarget(AimPosition);

            if (hasBarrels)
                RotateBarrelsToFaceTarget(AimPosition);

            // 포탑이 목표물을 조준하고 있을 때 "조준됨"으로 간주됩니다.
            angleToTarget = GetTurretAngleToTarget(AimPosition);

            // 포탑이 목표물을 조준하고 있을 때 "조준됨"으로 간주됩니다.
            isAimed = angleToTarget < aimedThreshold;

            isBarrelAtRest = false;
            isBaseAtRest = false;
        }
    }

    /// <summary>
    /// 주어진 목표 위치까지의 각도를 계산합니다.
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <returns>목표까지의 각도</returns>
    private float GetTurretAngleToTarget(Vector3 targetPosition)
    {
        float angle = 999f;

        if (hasBarrels)
        {
            angle = Vector3.Angle(targetPosition - barrels.position, barrels.forward);
        }
        else
        {
            Vector3 flattenedTarget = Vector3.ProjectOnPlane(
                targetPosition - turretBase.position,
                turretBase.up);

            angle = Vector3.Angle(
                flattenedTarget - turretBase.position,
                turretBase.forward);
        }

        return angle;
    }

    /// <summary>
    /// 포탑을 대기 상태로 회전시킵니다.
    /// </summary>
    private void RotateTurretToIdle()
    {
        // 베이스를 기본 위치로 회전시킵니다.
        if (hasLimitedTraverse)
        {
            limitedTraverseAngle = Mathf.MoveTowards(
                limitedTraverseAngle, 0f,
                TraverseSpeed * Time.deltaTime);

            if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
                turretBase.localEulerAngles = Vector3.up * limitedTraverseAngle;
            else
                isBaseAtRest = true;
        }
        else
        {
            turretBase.rotation = Quaternion.RotateTowards(
                turretBase.rotation,
                transform.rotation,
                TraverseSpeed * Time.deltaTime);

            isBaseAtRest = Mathf.Abs(turretBase.localEulerAngles.y) < Mathf.Epsilon;
        }

        if (hasBarrels)
        {
            elevation = Mathf.MoveTowards(elevation, 0f, ElevationSpeed * Time.deltaTime);
            if (Mathf.Abs(elevation) > Mathf.Epsilon)
                barrels.localEulerAngles = Vector3.right * -elevation;
            else
                isBarrelAtRest = true;
        }
        else // 총신이 없으면 자동으로 휴식 상태가 됩니다.
            isBarrelAtRest = true;
    }

    /// <summary>
    /// 포탑의 총신을 목표 위치로 회전시킵니다.
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    private void RotateBarrelsToFaceTarget(Vector3 targetPosition)
    {
        Vector3 localTargetPos = turretBase.InverseTransformDirection(targetPosition - barrels.position);
        Vector3 flattenedVecForBarrels = Vector3.ProjectOnPlane(localTargetPos, Vector3.up);

        float targetElevation = Vector3.Angle(flattenedVecForBarrels, localTargetPos);
        targetElevation *= Mathf.Sign(localTargetPos.y);

        targetElevation = Mathf.Clamp(targetElevation, -MaxDepression, MaxElevation);
        elevation = Mathf.MoveTowards(elevation, targetElevation, ElevationSpeed * Time.deltaTime);

        if (Mathf.Abs(elevation) > Mathf.Epsilon)
            barrels.localEulerAngles = Vector3.right * -elevation;

#if UNITY_EDITOR
        if (DrawDebugRay)
            Debug.DrawRay(barrels.position, barrels.forward * localTargetPos.magnitude, Color.red);
#endif
    }

    /// <summary>
    /// 목표를 감지합니다.
    /// </summary>
    private void DetectTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);

        Transform closestTarget = null;
        float closestDistance = detectionRange;

        foreach (Collider col in colliders)
        {
            Transform potentialTarget = col.transform;
            float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.position);
            Vector3 directionToTarget = (potentialTarget.position - transform.position).normalized;

            // 전방의 원뿔 형태로 감지
            if (Vector3.Dot(transform.forward, directionToTarget) > Mathf.Cos(Mathf.Deg2Rad * 45f)) // 45도 각도 내에 있는지 확인
            {
                if (distanceToTarget < closestDistance && !IsObstructed(potentialTarget.position))
                {
                    closestTarget = potentialTarget;
                    closestDistance = distanceToTarget;
                }
            }
        }

        target = closestTarget;

        if (target != null)
        {
            Debug.Log($"타겟 설정 완료: {target.name}");
            shoot.SetTarget(target.position);
            shoot.isShooting = true;
        }
        else
        {
            Debug.Log("타겟 찾기 실패!");
            shoot.isShooting = false;
        }
    }

    /// <summary>
    /// 목표 위치가 장애물에 의해 가려져 있는지 확인합니다.
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <returns>장애물이 있으면 true, 없으면 false</returns>
    private bool IsObstructed(Vector3 targetPosition)
    {
        // 포탑의 총신이 목표를 향하는 방향
        Vector3 direction = barrels.forward;
        float distance = Vector3.Distance(barrels.position, targetPosition);

        // 레이캐스트를 통해 장애물 감지
        if (Physics.Raycast(barrels.position, direction, distance, obstacleLayer))
        {
            return true; // 장애물이 있음
        }

        return false; // 장애물이 없음
    }

    /// <summary>
    /// 포탑의 베이스를 목표 위치로 회전시킵니다.
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    private void RotateBaseToFaceTarget(Vector3 targetPosition)
    {
        Vector3 turretUp = transform.up;

        Vector3 vecToTarget = targetPosition - turretBase.position;
        Vector3 flattenedVecForBase = Vector3.ProjectOnPlane(vecToTarget, turretUp);

        if (hasLimitedTraverse)
        {
            Vector3 turretForward = transform.forward;
            float targetTraverse = Vector3.SignedAngle(turretForward, flattenedVecForBase, turretUp);

            targetTraverse = Mathf.Clamp(targetTraverse, -LeftLimit, RightLimit);
            limitedTraverseAngle = Mathf.MoveTowards(
                limitedTraverseAngle,
                targetTraverse,
                TraverseSpeed * Time.deltaTime);

            if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
                turretBase.localEulerAngles = Vector3.up * limitedTraverseAngle;
        }
        else
        {
            turretBase.rotation = Quaternion.RotateTowards(
                Quaternion.LookRotation(turretBase.forward, turretUp),
                Quaternion.LookRotation(flattenedVecForBase, turretUp),
                TraverseSpeed * Time.deltaTime);
        }

#if UNITY_EDITOR
        if (DrawDebugRay && !hasBarrels)
            Debug.DrawRay(turretBase.position,
                turretBase.forward * flattenedVecForBase.magnitude,
                Color.red);
#endif
    }

#if UNITY_EDITOR
    // 이 코드는 Editor 스크립트에 있어야 하지만, Editor 스크립트를 다루는 것은 번거로우므로 여기서 처리합니다.
    private void OnDrawGizmosSelected()
    {
        if (!DrawDebugArcs)
            return;

        if (turretBase != null)
        {
            const float kArcSize = 10f;
            Color colorTraverse = new Color(1f, .5f, .5f, .1f);
            Color colorElevation = new Color(.5f, 1f, .5f, .1f);
            Color colorDepression = new Color(.5f, .5f, 1f, .1f);

            Transform arcRoot = barrels != null ? barrels : turretBase;

            // 빨간색 방위각 호
            UnityEditor.Handles.color = colorTraverse;
            if (hasLimitedTraverse)
            {
                UnityEditor.Handles.DrawSolidArc(
                    arcRoot.position, turretBase.up,
                    transform.forward, RightLimit,
                    kArcSize);
                UnityEditor.Handles.DrawSolidArc(
                    arcRoot.position, turretBase.up,
                    transform.forward, -LeftLimit,
                    kArcSize);
            }
            else
            {
                UnityEditor.Handles.DrawSolidArc(
                    arcRoot.position, turretBase.up,
                    transform.forward, 360f,
                    kArcSize);
            }

            if (barrels != null)
            {
                // 초록색 고각 호
                UnityEditor.Handles.color = colorElevation;
                UnityEditor.Handles.DrawSolidArc(
                    barrels.position, barrels.right,
                    turretBase.forward, -MaxElevation,
                    kArcSize);

                // 파란색 하각 호
                UnityEditor.Handles.color = colorDepression;
                UnityEditor.Handles.DrawSolidArc(
                    barrels.position, barrels.right,
                    turretBase.forward, MaxDepression,
                    kArcSize);
            }
        }
    }
#endif
}
