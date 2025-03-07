using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.Port;

// NPC의 AI 상태를 정의하는 열거형
public enum AIState
{
    Idle,       // 대기 상태
    Wandering,  // 배회 상태
    Attacking,  // 공격 상태
    Fleeing // 도망
}

public class NPC : MonoBehaviour, IDamageable
{
    NPC Instance;
    [Header("Stats")]
    public int health;                // NPC의 체력
    public float walkSpeed;           // 걷기 속도
    public float runSpeed;            // 달리기 속도
    public ItemData[] dropOnDeath;    // 죽을 때 드롭할 아이템들

    [Header("AI")]
    private NavMeshAgent agent;       // 네비게이션 에이전트
    public float detectDistance;      // 플레이어 감지 거리
    private AIState aiState;          // 현재 AI 상태

    [Header("Wandering")]
    public float minWanderDistance;   // 최소 배회 거리
    public float maxWanderDistance;   // 최대 배회 거리
    public float minWanderWaitTime;   // 최소 배회 대기 시간
    public float maxWanderWaitTime;   // 최대 배회 대기 시간

    [Header("Combat")]
    public int damage;                // 공격력
    public float attackRate;          // 공격 속도
    private float lastAttackTime;     // 마지막 공격 시간
    public float attackDistance;      // 공격 거리

    private float playerDistance;     // 플레이어와의 거리

    public float fieldOfView = 120f;  // 시야각
    public ItemData itemToGive;       // 줄 아이템
    private Animator animator;        // 애니메이터
    private SkinnedMeshRenderer[] meshRenderers; // 메쉬 렌더러 배열

    private void Awake()
    {
        // 컴포넌트 초기화
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        // 초기 상태 설정
        SetState(AIState.Wandering);
    }

    private void Update()
    {
        // 플레이어와의 거리 계산
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        // 애니메이터의 "Moving" 파라미터 설정
        animator.SetBool("Moving", aiState != AIState.Idle);

        // 현재 AI 상태에 따라 업데이트 메서드 호출
        switch (aiState)
        {
            case AIState.Idle:
                PassiveUpdate();
                break;
            case AIState.Wandering:
                PassiveUpdate();
                break;
            case AIState.Attacking:
                AttackingUpdate();
                break;
            case AIState.Fleeing:
                FleeingUpdate();
                break;
        }
    }

    // AI 상태 설정
    private void SetState(AIState state)
    {
        aiState = state;

        switch (aiState)
        {
            case AIState.Idle:
                agent.speed = walkSpeed;
                agent.isStopped = true;
                break;
            case AIState.Wandering:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attacking:
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
            case AIState.Fleeing:
                agent.speed = 0;
                agent.isStopped = true;
                break;
        }

        // 애니메이터 속도 설정
        animator.speed = agent.speed / walkSpeed;
    }

    // 대기 및 배회 상태 업데이트
    void PassiveUpdate()
    {
        if (aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }

        if (playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }

    // 새로운 위치로 배회
    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    // 배회할 위치 계산
    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);

        int i = 0;
        while (Vector3.Distance(transform.position, hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if (i == 30) break;
        }

        return hit.position;
    }

    // 피격 상태 업데이트
    void FleeingUpdate()
    {
        if (agent.remainingDistance < 0.1f) //거리가 가까워지면
        {
            agent.SetDestination(GetFleeLocation()); //받아온 값으로 도망친다.
        }
        else
        {
            SetState(AIState.Wandering);
        }
    }
    Vector3 GetFleeLocation()
    {
        NavMeshHit hit;

        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * detectDistance), out hit, maxWanderDistance, NavMesh.AllAreas);

        int i = 0;
        while (GetDestinationAngle(hit.position) > 90 || playerDistance < detectDistance)
        {

            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * detectDistance), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if (i == 30)
                break;
        }

        return hit.position;
    }

    float GetDestinationAngle(Vector3 targetPos)
    {
        return Vector3.Angle(transform.position - CharacterManager.Instance.Player.transform.position, transform.position + targetPos);
    }


    // 공격 상태 업데이트
    void AttackingUpdate()
    {
        if (playerDistance < attackDistance && IsPlayerFieldOfView())
        {
            agent.isStopped = true;
            if (Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;
                CharacterManager.Instance.Player.controller.GetComponent<IDamageable>().TakePhysicalDamage(damage);
                animator.speed = 1f;
                animator.SetTrigger("Attack");
            }
        }
        else
        {
            if (playerDistance < detectDistance)
            {
                agent.isStopped = false;
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path))
                {
                    agent.SetDestination(CharacterManager.Instance.Player.transform.position);
                }
                else
                {
                    agent.SetDestination(transform.position);
                    agent.isStopped = true;
                    SetState(AIState.Wandering);
                }
            }
            else
            {
                agent.SetDestination(transform.position);
                agent.isStopped = true;
                SetState(AIState.Wandering);
            }
        }
    }

    // 플레이어가 시야 내에 있는지 확인
    bool IsPlayerFieldOfView()
    {
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < fieldOfView * 0.5f;
    }

    // 물리적 피해 처리
    public void TakePhysicalDamage(float damage)
    {
        health -= (int)damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFlash());
        }
    }

    // NPC 사망 처리
    void Die()
    {
        for (int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // 피해 시 플래시 효과
    IEnumerator DamageFlash()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = Color.white;
        }
    }
}
