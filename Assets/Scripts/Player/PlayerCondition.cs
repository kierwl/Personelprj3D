using System;
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
            Debug.Log("허기로 인한 체력 감소");
        }

        if (health.curValue < 0f)
        {
            Die();
        }

        if (stamina.curValue <= 1f)
        {
            TakePhysicalDamage(10*Time.deltaTime);
            Debug.Log("스태미나가 부족하여 체력 감소");
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
        Debug.Log("플레이어가 죽었다.");
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
}