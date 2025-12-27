using UnityEngine;

public class BossSubCore : SubCore
{
    [SerializeField] BossAI boss;
    [SerializeField] CapsuleCollider2D col;

    [SerializeField] float groggyDamage;
    [SerializeField] float enhancingAmount;

    public float EnhancingAmount => enhancingAmount;

    protected override void Awake()
    {
        boss = FindAnyObjectByType<BossAI>();
        col = GetComponent<CapsuleCollider2D>();
    }

    public override void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            health = 0;
            boss.TakeGroggyDamage(groggyDamage);
            gameObject.SetActive(false);
        }
    }
}
