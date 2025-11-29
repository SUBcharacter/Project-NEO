using UnityEngine;

public class LongDiEnemy : MonoBehaviour
{
    [Header("Enemy 체력관련 변수")]
    [SerializeField] private int maxHits = 4;
    private int currentHits = 0;

    [Header("Enemy 이동 관련 변수")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveDistance = 2.5f;
    public bool isMovingRight = true;
    public Vector3 startPos;


    [Header("Enemy 총알 발사 관련 변수")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;

    // 내부 컴포넌트
    protected Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;

    [Header("Player 감지 관련 변수")]
    [SerializeField] private DetectionRange detectionRange;
    [SerializeField] private AttackRange attackRange;
    [SerializeField] private float fireCooldown = 1f;
    private float fireTimer = 0f;

    [Header("애니메이션 관련 변수")]
    private Animator animator;
    [SerializeField] private bool isAiming = false;
    [SerializeField] private bool isFiring = false;
    private float ReadytoFireTime = 0.5f;  // 조준으로 넘어갈 때 준비 시간

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();

        startPos = transform.position;
    }

    private void Update()
    {
        Debug.Log("aim: " + isAiming);

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
        if (isAiming || isFiring)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

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
        //플레이어가 시야 범위 안에 있는지 확인
        if (!detectionRange.isPlayerInRange)
        {
            isAiming = false;
            isFiring = false;
            return;
        }

        // 공격 범위 안에 플레이어가 있는지 확인
        if (!attackRange.isPlayerInAttackRange)
        {
            isAiming = false;
            isFiring = false;
            return;
        }

        if (fireTimer < fireCooldown) return;

        isAiming = true;
        isFiring = true;

        animator.SetTrigger("isAttack");
        // 총 쏘는 동안 제자리
        rigid.linearVelocity = Vector2.zero;

        Debug.Log("EnemtBullet launched attacked.");

        fireTimer = 0f;
    }

    void Fire()
    {
        // 플레이어의 마지막 위치를 기준으로 불렛 발사
        Vector3 lastTargetPos = attackRange.target.position;

        // 이건 총알이 제대로 날아가는지 테스트한 코드
        //Vector3 targetPos = attackRange.target.position;

        GameObject enemy = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

        EnemyBullet enemyBullet = enemy.GetComponent<EnemyBullet>();

        enemyBullet.Fire(firePoint.position, lastTargetPos);

        isAiming = false;
    }

    void TakeHit()
    {
        currentHits++;

        if ((currentHits >= maxHits))
        {
            Die();
        }
    }
    void Die()
    {
        gameObject.SetActive(false);
    }
}

