using System.Collections;
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
    [SerializeField] private int bulletCount = 4;
    [SerializeField] private float bulletInterval = 0.15f;   // 일단 좀 느리게
    private Vector3 fixedFirePointPosition;
    private Vector3 fixedEnemyPos;

    [Header("Player 감지 관련 변수")]
    [SerializeField] private DetectionRange detectionRange;
    [SerializeField] private AttackRange attackRange;

    [SerializeField] private float fireCooldown = 1f;
    private float fireTimer = 0f;

    [Header("애니메이션 관련 변수")]
    private Animator animator;
    private bool isAiming = false;
    private bool isFiring = false;
    private float readytoFireTime = 0.5f;
    private float readyTimer = 0f;
    private bool isReadyToFire = false;

    // 내부 컴포넌트
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        startPos = transform.position;
        fixedEnemyPos = transform.position;

        fixedFirePointPosition = firePoint.localPosition;
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
        // firePoint 좌우 반전 유지
        Vector3 newFirePos = fixedFirePointPosition;
        if (spriteRenderer.flipX)
            newFirePos.x *= -1f;

        firePoint.localPosition = newFirePos;

        // 공격 중엔 애니메이션이 위치 흔들지 못하게 고정
        if (isAiming || isFiring)
            transform.position = fixedEnemyPos;
        else
            fixedEnemyPos = transform.position;
    }

    private void Move()
    {
        float moveDir = isMovingRight ? 1f : -1f;
        rigid.linearVelocity = new Vector2(moveDir * moveSpeed, rigid.linearVelocity.y);

        float dist = transform.position.x - startPos.x;
        if (Mathf.Abs(dist) >= moveDistance)
        {
            isMovingRight = !isMovingRight;
            startPos = transform.position;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    private void TryFire()
    {
        if (isFiring) return;   // 중복 발사 완전 차단

        if (!detectionRange.isPlayerInRange || !attackRange.isPlayerInAttackRange)
        {
            isAiming = false;
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
            isFiring = true;   // 다음 공격 시작
            rigid.linearVelocity = Vector2.zero;

            animator.SetBool("isAttack", true);
            fireTimer = 0f;

            Fire();
        }
    }

    private void Fire()
    { 
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {

        for (int i = 0; i < bulletCount; i++)
        {
            Shoot();

            yield return CoroutineCasher.Wait(bulletInterval);
        }

        animator.SetBool("isAttack", false);
        isAiming = false;
        isReadyToFire = false;
        isFiring = false;     // 버스트 끝났으니 다음 공격 가능
    }

    private void Shoot()
    {
        if (firePoint == null)
            return;

        Vector3 targetPos = attackRange.target.position;
        Vector2 dir = (targetPos - firePoint.position).normalized;

        spriteRenderer.flipX = dir.x < 0;

        Vector3 newPos = fixedFirePointPosition;
        if (spriteRenderer.flipX)
            newPos.x *= -1f;
        firePoint.localPosition = newPos;

        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<EnemyBullet>().Fire(firePoint.position, targetPos);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBullet>() != null) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            TakeDamage();

        }
    }

    public void TakeDamage()
    {
        currentHits++;
        Debug.Log($"맞은 횟수 : {currentHits}");
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
