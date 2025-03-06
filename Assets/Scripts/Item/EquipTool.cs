using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate; // ���� �ӵ�
    private bool attacking; // ���� ������ ����
    public float attackDistance; // ���� �Ÿ�
    public float useStamina; // ���� �� �Ҹ�Ǵ� ���¹̳� ��

    [Header("Resource Gathering")]
    public bool doesGatherResources; // �ڿ� ä�� ���� ����

    [Header("Combat")]
    public bool doesDealDamage; // ������ ���� �� �ִ��� ����
    public int damage; // ������ ��

    private Animator animator; // �ִϸ����� ������Ʈ
    private Camera camera; // ���� ī�޶�

    private void Start()
    {
        animator = GetComponent<Animator>(); // �ִϸ����� ������Ʈ ��������
        camera = Camera.main; // ���� ī�޶� ��������
    }

    public override void OnAttackInput()
    {
        if (!attacking) // ���� ���� �ƴ� ���� ����
        {
            if (CharacterManager.Instance.Player.condition.UseStamina(useStamina)) // ���¹̳� ��� ���� ���� Ȯ��
            {
                attacking = true; // ���� ���·� ��ȯ
                animator.SetTrigger("Attack"); // ���� �ִϸ��̼� Ʈ���� ����
                Invoke("OnCanAttack", attackRate); // ���� �ð� �� ���� ���� ���·� ��ȯ
            }
        }
    }

    void OnCanAttack()
    {
        attacking = false; // ���� ���� ����
    }

    public void OnHit()
    {
        // ȭ�� �߾ӿ��� ����ĳ��Ʈ �߻�
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackDistance)) // ����ĳ��Ʈ�� �浹���� ��
        {
            if (doesGatherResources && hit.collider.TryGetComponent(out Resource resource)) // �ڿ� ä�� ���� ���� Ȯ�� �� �ڿ� ������Ʈ ��������
            {
                resource.Gather(hit.point, hit.normal); // �ڿ� ä��
            }
            else if (doesDealDamage && hit.collider.TryGetComponent(out IDamageable damageable)) // ������ ���� �� �ִ��� ���� Ȯ�� �� ����� ������ ������Ʈ ��������
            {
                damageable.TakePhysicalDamage(damage); // ������ ���� ����
            }
        }
    }
}
