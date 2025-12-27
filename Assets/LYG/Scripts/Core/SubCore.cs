using UnityEngine;

public class SubCore : MonoBehaviour,IDamageable
{
    [SerializeField] MainCore main;
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;

    protected virtual void Awake()
    {
        main = FindAnyObjectByType<MainCore>();
        health = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if(health <= 0)
        {
            health = 0;
            gameObject.SetActive(false);
            main.DestorySubCore();
        }
    }
}
