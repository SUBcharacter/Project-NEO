using UnityEngine;

public abstract class Enemy : MonoBehaviour, IResetable, IDamageable
{
    [Header("Enemy 데이터")]
    [SerializeField] protected EnemyData enemyData;

    // 런타임 상태값
    protected bool isMovingRight = true;
    protected Vector3 startPos;
    protected int currentHits = 0;

    // 컴포넌트
    protected Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;

    // 기본 Awake
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Init(); // 체력, 위치 등 초기화
    }

    // 초기화 (원하면 override 가능)
    public virtual void Init()
    {
        currentHits = 0;
        isMovingRight = true;
        startPos = transform.position;
    }

    public virtual void TakeDamage(float damage)
    {

    }

    // 공통 데미지 처리
    public virtual void TakeDamage()
    {
        currentHits++;
        Debug.Log($"맞은 횟수 : " + currentHits);

        if (currentHits >= enemyData.maxHits)
            Die();
    }

    // 공통 사망 처리
    public virtual void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("뒤짐");
    }

    // 적마다 구현하는 행동
    protected abstract void Move();
    protected abstract void Attack();
    protected abstract void Chase();
}
