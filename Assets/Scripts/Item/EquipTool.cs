using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate; // 공격 속도
    private bool attacking; // 공격 중인지 여부
    public float attackDistance; // 공격 거리
    public float useStamina; // 공격 시 소모되는 스태미나 양

    [Header("Resource Gathering")]
    public bool doesGatherResources; // 자원 채집 가능 여부

    [Header("Combat")]
    public bool doesDealDamage; // 데미지 가할 수 있는지 여부
    public int damage; // 데미지 양

    private Animator animator; // 애니메이터 컴포넌트
    private Camera camera; // 메인 카메라

    private void Start()
    {
        animator = GetComponent<Animator>(); // 애니메이터 컴포넌트 가져오기
        camera = Camera.main; // 메인 카메라 가져오기
    }

    public override void OnAttackInput()
    {
        if (!attacking) // 공격 중이 아닐 때만 실행
        {
            if (CharacterManager.Instance.Player.condition.UseStamina(useStamina)) // 스태미나 사용 가능 여부 확인
            {
                attacking = true; // 공격 상태로 전환
                animator.SetTrigger("Attack"); // 공격 애니메이션 트리거 설정
                Invoke("OnCanAttack", attackRate); // 일정 시간 후 공격 가능 상태로 전환
            }
        }
    }

    void OnCanAttack()
    {
        attacking = false; // 공격 상태 해제
    }

    public void OnHit()
    {
        // 화면 중앙에서 레이캐스트 발사
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackDistance)) // 레이캐스트가 충돌했을 때
        {
            if (doesGatherResources && hit.collider.TryGetComponent(out Resource resource)) // 자원 채집 가능 여부 확인 및 자원 컴포넌트 가져오기
            {
                resource.Gather(hit.point, hit.normal); // 자원 채집
            }
            else if (doesDealDamage && hit.collider.TryGetComponent(out IDamageable damageable)) // 데미지 가할 수 있는지 여부 확인 및 대미지 가능한 컴포넌트 가져오기
            {
                damageable.TakePhysicalDamage(damage); // 물리적 피해 적용
            }
        }
    }
}
