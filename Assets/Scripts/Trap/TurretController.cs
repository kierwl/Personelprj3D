using UnityEngine;

namespace GT2.Demo
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private TurretAim TurretAim = null;

        public Transform TargetPoint = null;

        private bool isIdle = false;

        private void Awake()
        {
            if (TurretAim == null)
                Debug.LogError(name + ": TurretController에 TurretAim이 할당되지 않았습니다!");
        }

        private void Update()
        {
            if (TurretAim == null)
                return;

            if (TargetPoint == null)
                TurretAim.IsIdle = TargetPoint == null;
            else
                TurretAim.AimPosition = TargetPoint.position;

            if (Input.GetMouseButtonDown(0))
                TurretAim.IsIdle = !TurretAim.IsIdle;
        }
    }
}
