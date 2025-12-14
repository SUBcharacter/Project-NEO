using UnityEngine;

public abstract class Enemy : MonoBehaviour, IResetable, IDamageable
{
    [Header("Enemy 데이터")]
    [SerializeField] protected EnemyData enemyData;
    protected float currnetHealth;
    // 런타임 상태값

    // 컴포넌트
    public Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;

    // 기본 Awake
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Init(); // 체력
    }

    // 초기화 (원하면 override 가능)
    public virtual void Init()
    {
        currnetHealth = enemyData.MaxHp;
    }
    
    // 알아서 수정할 것
    public virtual void TakeDamage(float damage){ }

    // 공통 사망 처리
    public virtual void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("뒤짐"); // ㅋㅋㅋ
    }

    // 적마다 구현하는 행동
    public abstract void Move();
    public abstract void Attack();
    public abstract void Chase();
}
