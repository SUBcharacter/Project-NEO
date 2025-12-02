using System.Collections;
using UnityEngine;

public class LongDiEnemy : LongDiEnemyBase
{
    [Header("Enemy 총알 발사 관련 변수")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletCount = 4;                // 한 번에 발사할 총알 개수
    [SerializeField] private float bulletInterval = 0.15f;       // 총알 간 간격 (연사 속도)
    private Vector3 fixedFirePointPosition;                      // 원본 firePoint 위치 저장 (좌우 반전 시 보정용)
    private Vector3 fixedEnemyPos;                               // 애니메이션 중 위치 흔들림 방지용 고정 좌표

    [Header("Player 감지 관련 변수")]
    [SerializeField] private DetectionRange detectionRange;      // 플레이어 감지 범위
    [SerializeField] private AttackRange attackRange;            // 공격 가능 범위

    [SerializeField] private float fireCooldown = 1f;            // 공격 쿨타임 (연사 후 대기 시간)
    private float fireTimer = 0f;                                // 쿨타임 타이머

    [Header("애니메이션 관련 변수")]
    private Animator animator;
    private bool isAiming = false;                               // 조준 상태 여부
    private bool isFiring = false;                               // 현재 발사 중인지 여부
    private float readytoFireTime = 0.5f;                        // 조준 완료까지 걸리는 시간
    private float readyTimer = 0f;                               // 조준 시간 누적용 타이머
    private bool isReadyToFire = false;                          // 조준 완료 플래그

    private void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();

        // 초기 위치값 고정 (애니메이션 중 좌표 틀어짐 방지)
        fixedEnemyPos = transform.position;
        fixedFirePointPosition = firePoint.localPosition;
    }

    private void Update()
    {
        // 공격 쿨타임 갱신 및 사격 조건 체크
        fireTimer += Time.deltaTime;
        TryFire();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void LateUpdate()
    {
        HandleFirePointFlip();
        HandlePositionLock();
    }

    /// <summary>
    /// 플레이어가 공격 범위 안에 들어왔을 때 공격 시도 (AI 메인 루프)
    /// </summary>
    private void TryFire()
    {
        if (isFiring) return;           // 이미 발사 중이면 중복 실행 방지
        if (!CanSeePlayer()) return;    // 플레이어가 범위 밖이면 초기화 후 종료

        HandleAim();                    // 조준 처리

        if (CanFire())                  // 발사 조건 충족 시 공격 시작
            StartFire();
    }

    /// <summary>
    /// 플레이어 감지 여부 확인 및 감지 실패 시 상태 초기화
    /// </summary>
    private bool CanSeePlayer()
    {
        if (!detectionRange.isPlayerInRange || !attackRange.isPlayerInAttackRange)
        {
            isAiming = false;
            isReadyToFire = false;
            readyTimer = 0f;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 조준 처리 (조준 완료까지 대기 후 발사 준비)
    /// </summary>
    private void HandleAim()
    {
        if (!isReadyToFire)
        {
            isAiming = true;
            rigid.linearVelocity = Vector2.zero;
            readyTimer += Time.deltaTime;

            if (readyTimer >= readytoFireTime)
            {
                isReadyToFire = true; // 조준 완료
                readyTimer = 0f;
            }
        }
    }

    /// <summary>
    /// 발사 조건 확인 (쿨타임 + 조준 완료)
    /// </summary>
    private bool CanFire()
    {
        return fireTimer >= fireCooldown && isReadyToFire;
    }

    /// <summary>
    /// 공격 시작 처리 (애니메이션 트리거 및 코루틴 호출)
    /// </summary>
    private void StartFire()
    {
        isFiring = true;
        rigid.linearVelocity = Vector2.zero;

        animator.SetBool("isAttack", true);
        fireTimer = 0f;

        Fire(); // 연사 코루틴 시작
    }

    /// <summary>
    /// 이동 처리 (공격 중일 땐 정지)
    /// </summary>
    private void HandleMovement()
    {
        if (!isAiming && !isFiring)
            Move();
        else
            rigid.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// firePoint 좌우 반전 유지 (적 방향에 맞춰 보정)
    /// </summary>
    private void HandleFirePointFlip()
    {
        Vector3 newFirePos = fixedFirePointPosition;
        if (spriteRenderer.flipX)
            newFirePos.x *= -1f;

        firePoint.localPosition = newFirePos;
    }

    /// <summary>
    /// 공격 중일 때 적의 실제 위치를 고정 (애니메이션 흔들림 방지)
    /// </summary>
    private void HandlePositionLock()
    {
        if (isAiming || isFiring)
            transform.position = fixedEnemyPos;
        else
            fixedEnemyPos = transform.position;
    }

    /// <summary>
    /// 발사 코루틴 시작 (연사 루프 처리)
    /// </summary>
    private void Fire()
    {
        StartCoroutine(FireRoutine());
    }

    /// <summary>
    /// bulletCount만큼 총알을 순차 발사하고 딜레이 후 상태 초기화
    /// </summary>
    private IEnumerator FireRoutine()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            Shoot();
            yield return CoroutineCasher.Wait(bulletInterval); // 코루틴 캐셔 사용
        }

        // 공격 후 상태 초기화
        animator.SetBool("isAttack", false);
        isAiming = false;
        isReadyToFire = false;
        isFiring = false;

        fireTimer = 0f;
    }

    /// <summary>
    /// 실제 총알 발사 처리 (firePoint 위치 기준)
    /// </summary>
    private void Shoot()
    {
        if (firePoint == null)
            return;

        Vector3 targetPos = attackRange.target.position;
        Vector2 dir = (targetPos - firePoint.position).normalized;

        // 방향에 따라 좌우 반전 적용
        spriteRenderer.flipX = dir.x < 0;

        // firePoint의 위치 보정
        Vector3 newPos = fixedFirePointPosition;
        if (spriteRenderer.flipX)
            newPos.x *= -1f;
        firePoint.localPosition = newPos;

        // 총알 프리팹 생성 및 발사
        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<EnemyBullet>().Fire(firePoint.position, targetPos);
    }

    /// <summary>
    /// firePoint 위치 시각화 (디버깅용)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.05f);
        }
    }
}
