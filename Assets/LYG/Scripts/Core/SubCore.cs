using UnityEngine;

public class SubCore : MonoBehaviour,IDamageable
{
    [SerializeField] MainCore main;
    [SerializeField] float health;
    [SerializeField] float maxHealth;

    private void Awake()
    {
        main = FindAnyObjectByType<MainCore>();
        health = maxHealth;
    }

    public void TakeDamage(float damage)
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
