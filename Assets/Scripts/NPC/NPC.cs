using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.Port;

// NPC�� AI ���¸� �����ϴ� ������
public enum AIState
{
    Idle,       // ��� ����
    Wandering,  // ��ȸ ����
    Attacking,  // ���� ����
    Fleeing // ����
}

public class NPC : MonoBehaviour, IDamageable
{
    NPC Instance;
    [Header("Stats")]
    public int health;                // NPC�� ü��
    public float walkSpeed;           // �ȱ� �ӵ�
    public float runSpeed;            // �޸��� �ӵ�
    public ItemData[] dropOnDeath;    // ���� �� ����� �����۵�

    [Header("AI")]
    private NavMeshAgent agent;       // �׺���̼� ������Ʈ
    public float detectDistance;      // �÷��̾� ���� �Ÿ�
    private AIState aiState;          // ���� AI ����

    [Header("Wandering")]
    public float minWanderDistance;   // �ּ� ��ȸ �Ÿ�
    public float maxWanderDistance;   // �ִ� ��ȸ �Ÿ�
    public float minWanderWaitTime;   // �ּ� ��ȸ ��� �ð�
    public float maxWanderWaitTime;   // �ִ� ��ȸ ��� �ð�

    [Header("Combat")]
    public int damage;                // ���ݷ�
    public float attackRate;          // ���� �ӵ�
    private float lastAttackTime;     // ������ ���� �ð�
    public float attackDistance;      // ���� �Ÿ�

    private float playerDistance;     // �÷��̾���� �Ÿ�

    public float fieldOfView = 120f;  // �þ߰�
    public ItemData itemToGive;       // �� ������
    private Animator animator;        // �ִϸ�����
    private SkinnedMeshRenderer[] meshRenderers; // �޽� ������ �迭

    private void Awake()
    {
        // ������Ʈ �ʱ�ȭ
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        // �ʱ� ���� ����
        SetState(AIState.Wandering);
    }

    private void Update()
    {
        // �÷��̾���� �Ÿ� ���
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        // �ִϸ������� "Moving" �Ķ���� ����
        animator.SetBool("Moving", aiState != AIState.Idle);

        // ���� AI ���¿� ���� ������Ʈ �޼��� ȣ��
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

    // AI ���� ����
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

        // �ִϸ����� �ӵ� ����
        animator.speed = agent.speed / walkSpeed;
    }

    // ��� �� ��ȸ ���� ������Ʈ
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

    // ���ο� ��ġ�� ��ȸ
    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    // ��ȸ�� ��ġ ���
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

    // �ǰ� ���� ������Ʈ
    void FleeingUpdate()
    {
        if (agent.remainingDistance < 0.1f) //�Ÿ��� ���������
        {
            agent.SetDestination(GetFleeLocation()); //�޾ƿ� ������ ����ģ��.
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


    // ���� ���� ������Ʈ
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

    // �÷��̾ �þ� ���� �ִ��� Ȯ��
    bool IsPlayerFieldOfView()
    {
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < fieldOfView * 0.5f;
    }

    // ������ ���� ó��
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

    // NPC ��� ó��
    void Die()
    {
        for (int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // ���� �� �÷��� ȿ��
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
