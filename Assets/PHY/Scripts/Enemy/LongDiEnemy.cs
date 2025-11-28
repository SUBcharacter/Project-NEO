using UnityEngine;

public class LongDiEnemy : MonoBehaviour
{
    [Header("Enemy 이동 관련 변수")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveDistance = 2.5f;
    public bool isMovingRight = true;
    public Vector3 startPos;


    [Header("Enemy 총알 발사 관련 변수")]
    [SerializeField] private EnemyBullet enemyBullet;
    [SerializeField] private Transform firePoint;

    // 내부 컴포넌트
    protected Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;

    [Header("Player 감지 관련 변수")]
    [SerializeField] private DetectionRange detectionRange;
    [SerializeField] private AttackRange attackRange;
    [SerializeField] private float fireCooldown = 1f;
    private float fireTimer = 0f;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        startPos = transform.position;
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;
        TryFire();
    }

    void FixedUpdate()
    {
        Move();
    }

    // 에너미 이동 함수
    private void Move()
    {
        float moveDir = isMovingRight ? 1f : -1f;

        rigid.linearVelocity = new Vector2(moveDir * moveSpeed, rigid.linearVelocity.y);

        float distanceMoved = transform.position.x - startPos.x;

        if (Mathf.Abs(distanceMoved) >= moveDistance)
        {
            isMovingRight = !isMovingRight;

            startPos = transform.position;

            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    void TryFire()
    {
        // 플레이어가 시야 범위안에 있는지 확인
        if (!detectionRange.isPlayerInRange) return;

        // 플레이어가 공격 범위안에 있는지 확인
        if (!attackRange.isPlayerInAttackRange) return;

        if (fireTimer < fireCooldown) return;

        fireTimer = 0f;

        Fire();
    }

    void Fire()
    {
        Vector3 targetPos = attackRange.target.position;

        EnemyBullet E = Instantiate(enemyBullet);

        E.Fire(firePoint.position, targetPos);
    }
}

