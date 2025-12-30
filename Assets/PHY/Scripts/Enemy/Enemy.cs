using UnityEngine;

public enum EnemyTypeState
{
    Idle, Attack, Dead, Hit, Walk, Chase, Enhance, Return, Summon
}
public abstract class Enemy : MonoBehaviour, IResetable, IDamageable
{
    [Header("Enemy 데이터")]
    [SerializeField] protected EnemyData stat;
    [SerializeField] protected float currnetHealth;
    // 런타임 상태값

    [SerializeField] protected Vector3 startPos;
    [SerializeField] protected bool facingRight;

    // 컴포넌트
    [SerializeField] protected Rigidbody2D rigid;
    [SerializeField] protected SpriteRenderer ren;

    public EnemyData Stat => stat;
    public Rigidbody2D Rigid { get => rigid; set => rigid = value; }
    public SpriteRenderer Ren { get => ren; set => ren = value; }

    public bool FacingRight => facingRight;

    // 기본 Awake
    protected abstract void Awake();


    // 초기화 (원하면 override 가능)
    public abstract void Init();

    // 알아서 수정할 것
    public abstract void TakeDamage(float damage);

    // 공통 사망 처리
    protected abstract void Die();

    public abstract void Attack();
}

