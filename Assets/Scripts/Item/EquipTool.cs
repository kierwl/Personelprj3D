using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate;
    private bool attacking;
    public float attackDistance;

    [Header("Resource Gathering")]
    public bool doesGatherResources;

    [Header("Combat")]
    public bool doesDealDamage;
    public int damage;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public override void OnAttackInput()
    {
        if(!attacking)
        {
            attacking = true;
            animator.SetTrigger("Attack");
            Invoke("ResetAttack", attackRate);
        }
    }

    void OnCanAttack()
    {
        attacking = false;
    }
}
