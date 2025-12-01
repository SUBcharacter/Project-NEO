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
    private Vector3 fixedFirePointPosition;
    private Vector3 fixedEnemyPos;

    [Header("Player 감지 관련 변수")]
    [SerializeField] private DetectionRange detectionRange;
    [SerializeField] private AttackRange attackRange;
    [SerializeField] private float fireCooldown = 1f;
    private float fireTimer = 0f;

    [Header("애니메이션 관련 변수")]
    private Animator animator;
    [SerializeField] private bool isAiming = false;
    [SerializeField] private bool isFiring = false;
    private float readytoFireTime = 0.5f;  // 조준으로 넘어갈 때 준비 시간
    private float readyTimer = 0f;
    private bool isReadyToFire = false;

    // 내부 컴포넌트
    protected Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        startPos = transform.position;
        fixedEnemyPos = transform.position;


        fixedFirePointPosition = firePoint.localPosition;  
        Debug.Log("firepoint 위치 : " + firePoint.localPosition);
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;
        TryFire();
    }

    private void FixedUpdate()
    {
        if (!isAiming && !isFiring)
            Move();
        else
            rigid.linearVelocity = Vector2.zero;
    }

    private void LateUpdate()
    { 
        Vector3 newFirePos = fixedFirePointPosition;

        if (spriteRenderer.flipX)
        {
            newFirePos.x *= -1f;
        }

        firePoint.localPosition = newFirePos;


        // 애니메이터가 위치 흔드는 거 막기
        if (isAiming || isFiring)
        {
            transform.position = fixedEnemyPos;
        }
        else
        { 
            fixedEnemyPos = transform.position; 
        }

    }

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

    private void TryFire()
    {
        if (!detectionRange.isPlayerInRange || !attackRange.isPlayerInAttackRange)
        {
            isAiming = false;
            isFiring = false;
            isReadyToFire = false;
            readyTimer = 0f;
            return;
        }

        if (!isReadyToFire)
        {
            isAiming = true;
            rigid.linearVelocity = Vector2.zero;
            readyTimer += Time.deltaTime;

            if (readyTimer >= readytoFireTime)
            {
                isReadyToFire = true;
                readyTimer = 0f;
            }
        }

        if (fireTimer >= fireCooldown && isReadyToFire)
        {
            isFiring = true;
            rigid.linearVelocity = Vector2.zero;
            animator.SetBool("isAttack", true);
            fireTimer = 0f;
        }
    }

    private void Fire()
    {
        if (firePoint == null)
        {
            Debug.LogError("firePoint가 null임.");
            return;
        }

        Vector3 lastTargetPos = attackRange.target.position;

        Debug.Log("공격 대상 : " + attackRange.target.name);
        Vector2 direction = (lastTargetPos - firePoint.position).normalized;

        spriteRenderer.flipX = direction.x < 0;

        // firePoint 위치를 바라보는 방향에 따라 좌우 반전
        Vector3 newFirePos = fixedFirePointPosition;

        if (spriteRenderer.flipX)
        {
            newFirePos.x *= -1f;
        }

        firePoint.localPosition = newFirePos;

        Debug.Log("에너미 불렛 현재 발사 위치 : " + firePoint.localPosition);

        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<EnemyBullet>().Fire(firePoint.position, lastTargetPos);

     

        isAiming = false;
        isFiring = false;
        isReadyToFire = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<EnemyBullet>() != null)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log($"충돌 감지: {collision.gameObject.name}, 레이어: {collision.gameObject.layer}");

            TakeDamage();
            Debug.Log("요원이 플레이어에게 맞음");
        }
    }

    public void TakeDamage()
    {
        currentHits++;
        if (currentHits >= maxHits)
            Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("요원 사망");
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.05f);
        }
    }

}

