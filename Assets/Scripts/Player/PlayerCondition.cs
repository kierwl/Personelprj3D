using System;
using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakePhysicalDamage(float damageAmount);
}
public class PlayerCondition : MonoBehaviour, IDamageable
{
    public UICondition uiCondition;

    Condition health { get { return uiCondition.health; } }
    Condition hunger { get { return uiCondition.hunger; } }
    Condition stamina { get { return uiCondition.stamina; } }

    public float noHungerHealthDecay;
    public event Action onTakeDamage;

   

    private void Update()
    {
        hunger.Subtract(hunger.passiveValue * Time.deltaTime);
        stamina.Add(stamina.passiveValue * Time.deltaTime);

        if (hunger.curValue <= 0f)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
            Debug.Log("���� ���� ü�� ����");
        }

        if (health.curValue < 0f)
        {
            Die();
        }

        if (stamina.curValue <= 1f)
        {
            TakePhysicalDamage(10*Time.deltaTime);
            Debug.Log("���¹̳��� �����Ͽ� ü�� ����");
        }
    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Eat(float amount)
    {
        hunger.Add(amount);
    }

    public void Die()
    {

        Debug.Log("�÷��̾ �׾���.");
        StartCoroutine(Respawn());
    }

    public void TakePhysicalDamage(float damageAmount)
    {
        health.Subtract(damageAmount);
        onTakeDamage?.Invoke();
    }

    public bool UseStamina(float amount)
    {
        if (stamina.curValue - amount <= 0f)
        {
            return false;
        }
        stamina.Subtract(amount);
        return true;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3f); // 3�� ���

        // �÷��̾� ���� �ʱ�ȭ
        health.curValue = health.maxValue;
        hunger.curValue = hunger.startValue;
        stamina.curValue = stamina.startValue;

        // UI ������Ʈ
        uiCondition.health.uiBar.fillAmount = health.GetPercentage();
        uiCondition.hunger.uiBar.fillAmount = hunger.GetPercentage();
        uiCondition.stamina.uiBar.fillAmount = stamina.GetPercentage();

        // �÷��̾� ��ġ �ʱ�ȭ (��: ���� ��ġ�� �̵�)
        transform.position = Vector3.zero; // ���ϴ� ��ġ�� ����

        Debug.Log("�÷��̾ ����۵Ǿ����ϴ�.");
    }
}