using System.Collections;
using UnityEngine;

// 리셋이 가능하게 IResetable 만들어서 초기화 할 수 잇게 수정하기 
// 변수들은 스크립터블로 만들어서 관리하기
// 충돌 감지는 트리거가 아닌 오버랩 스피어를 사용하기 
// 시야범위, 공격범위 따로 두지말고 하나로 합쳐서 레이어마스크로 처리하기
// 에너미 추상 클래스 만들어서 처리하도록 수정하기(오버라이딩 이용하라는거 같음)
// IResetable 인터페이스를 내가 직접 만들어야하는거임? 시발? 일단 이것도 추후에 하기
// 히트박스 스크립트가 유틸 스크립트로 있으니 일단 써봐도 될거같음 (맨마지막에 해보기)

// 적 상태에 관련한 부분은 PD와 의논할 것, 필요시 도와줌

//public class LongDiEnemy : Enemy
//{
//    [Header("컴포넌트 & 참조 관련 변수")]
//    [SerializeField] private DetectionSensor sensor;   // 플레이어 감지 센서
//    [SerializeField] private Transform player;
//    private Animator animator;
//    private Transform targetPlayer = null;             // 타겟 플레이어 위치
//
//    [Header("프리팹 & 위치 관련")]
//    [SerializeField] private GameObject enemyBulletPrefab;
//    [SerializeField] private Transform firePoint;
//
//    [Header("위치 & 연산용 변수")]
//    private Vector3 fixedFirePointPosition;            // 좌우 반전 대비 firePoint 원본 로컬 위치
//    private Vector3 fixedEnemyPos;                     // 애니메이션 중 위치 흔들림 방지용
//
//    [Header("시간 관련 변수")]
//    private float fireTimer = 0f;                      // 발사 쿨타임 타이머
//    private float readyTimer = 0f;                     // 조준 시간 누적
//
//    [Header("상태 플래그(boolean)")]
//    private bool isAiming = false;                     // 조준 중인지
//    private bool isFiring = false;                     // 발사 중인지
//    private bool isReadyToFire = false;                // 조준 완료 여부
//    private bool isAttackMode = false;                 // 공격 모드 여부
//    private bool isChaseMode = false;                  // 추적 모드 여부
//
//    protected override void Awake()
//    {
//        base.Awake();
//        animator = GetComponentInChildren<Animator>();
//
//        // 초기 위치값 고정 (애니메이션 중 좌표 틀어짐 방지)
//        fixedEnemyPos = transform.position;
//        fixedFirePointPosition = firePoint.localPosition;
//    }
//
//    private void Update()
//    {
//        // 최초 감지할 때만 공격 
//        if (!isAttackMode && sensor.IsPlayerDetected)
//        {
//            isAttackMode = true;
//            targetPlayer = sensor.detectedPlayer;
//        }
//
//        fireTimer += Time.deltaTime;
//
//        TryAim();
//        TryFire();
//    }
//
//    private void FixedUpdate()
//    {
//        if (isAttackMode)
//        {
//            rigid.linearVelocity = Vector2.zero;
//            return;
//        }
//
//        if (isChaseMode && targetPlayer != null)
//        {
//            Chase();
//            return;
//        }
//        HandleMove();
//    }
//
//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        //Debug.Log("플레이어 총알이 감지됨");
//        // 플레이어 총알 레이어(Default)만 맞았을 때
//        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
//        {
//            // 적의 총알인지 체크 (같은 Default라서 분리 필요)
//            if (collision.GetComponent<EnemyBullet>() != null)
//                return; // 적 총알 → 무시
//
//            TakeDamage(5f);
//            Destroy(collision.gameObject);
//
//            // 이미 공격 모드면 추적 불필요
//            if (!isAttackMode)
//            {
//                HitEnemy();
//                Debug.Log("플레이어 추적시작");
//            }
//        }
//    }
//    /// <summary>
//    /// 언제 리셋할지 모르기 때문에 만들어놓기만하고 사용은 추후에 할 예정
//    /// </summary>
//    public override void Init()
//    {
//        base.Init();
//
//        isAiming = false;
//        isFiring = false;
//        isReadyToFire = false;
//        isAttackMode = false;
//        isChaseMode = false;
//
//        targetPlayer = null;
//
//        fireTimer = 0f;
//        readyTimer = 0f;
//
//        rigid.linearVelocity = Vector2.zero;
//
//        firePoint.localPosition = fixedFirePointPosition;
//    }
//
//
//    void TryAim()
//    {
//        // 공격 모드가 아니면 조준 초기화
//        if (!isAttackMode)
//        {
//            isAiming = false;
//            isFiring = false;
//            isReadyToFire = false;
//            readyTimer = 0f;
//            return;
//        }
//
//        HandleAim();
//
//    }
//
//    /// <summary>
//    /// 플레이어가 공격 범위 안에 들어왔을 때 공격 시도 (AI 메인 루프)
//    /// </summary>
//    private void TryFire()
//    {
//        if (!isAttackMode) return;
//        if (isFiring) return;
//        if (!isReadyToFire) return;
//        if (targetPlayer == null) return;
//
//        if (fireTimer < enemyData.fireCooldown) return;
//
//        StartFire();
//    }
//
//    /// <summary>
//    /// 조준 처리 (조준 완료까지 대기 후 발사 준비)
//    /// </summary>
//    private void HandleAim()
//    {
//        if (isReadyToFire) return;
//
//        isAiming = true;
//        rigid.linearVelocity = Vector2.zero;
//
//        readyTimer += Time.deltaTime;
//
//        if (readyTimer >= enemyData.readyToFireTime)
//        {
//            isReadyToFire = true;
//            readyTimer = 0f;
//        }
//    }
//
//    /// <summary>
//    /// 이동 처리 (공격 중일 땐 정지)
//    /// </summary>
//    private void HandleMove()
//    {
//        if (!isAiming && !isFiring)
//            Move();
//        else
//            rigid.linearVelocity = Vector2.zero;
//    }
//
//    /// <summary>
//    /// 공격 시작 처리 (애니메이션 트리거 및 코루틴 호출)
//    /// </summary>
//    private void StartFire()
//    {
//        isFiring = true;
//        rigid.linearVelocity = Vector2.zero;
//
//        animator.SetBool("isAttack", true);
//        fireTimer = 0f;
//
//        Fire(); // 연사 코루틴 시작
//    }

    /// <summary>
    /// 발사 코루틴 시작 (연사 루프 처리)
    /// </summary>
//    private void Fire()
//    {
//        StartCoroutine(FireRoutine());
//    }
//
//    /// <summary>
//    /// bulletCount만큼 총알을 순차 발사하고 딜레이 후 상태 초기화
//    /// </summary>
//    private IEnumerator FireRoutine()
//    {
//        for (int i = 0; i < enemyData.bulletCount; i++)
//        {
//            Shoot();
//            yield return CoroutineCasher.Wait(enemyData.bulletInterval); // 코루틴 캐셔 사용
//        }
//
//        // 공격 후 상태 초기화
//        animator.SetBool("isAttack", false);
//        isAiming = false;
//        isReadyToFire = false;
//        isFiring = false;
//
//        fireTimer = 0f;
//    }
//
//    /// <summary>
//    /// 실제 총알 발사 처리 (firePoint 위치 기준)
//    /// </summary>
//    private void Shoot()
//    {
//        if (firePoint == null || targetPlayer == null)
//            return;
//
//        Vector3 targetPos = targetPlayer.position;
//        Vector2 dir = (targetPos - firePoint.position).normalized;
//
//        // 방향에 따라 좌우 반전 적용
//        spriteRenderer.flipX = dir.x < 0;
//
//        // firePoint의 위치 보정
//        Vector3 newPos = fixedFirePointPosition;
//        if (spriteRenderer.flipX)
//            newPos.x *= -1f;
//
//        firePoint.localPosition = newPos;
//
//        // 총알 프리팹 생성 및 발사
//        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
//        bullet.GetComponent<EnemyBullet>().Fire(firePoint.position, targetPos);
//    }
//
//    public void HitEnemy()
//    {
//        if (isAttackMode) return;
//
//
//        Transform dectected = sensor != null ? sensor.GetDetectedPlayer() : null;
//
//
//        if (dectected == null)
//        {
//            Vector2 dir = player.position - transform.position;
//            float distance = Vector2.Distance(transform.position, player.position);
//
//            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, sensor.obstacleLayer);
//
//            if (hit) return;
//
//            isChaseMode = true;
//            targetPlayer = player;
//            return;
//        }
//        isChaseMode = true;
//
//        targetPlayer = dectected;
//    }
//
//    #region Enemy 추상 클래스 구현
//    public override void Move()
//    {
//        float moveDir = isMovingRight ? 1f : -1f;
//        rigid.linearVelocity = new Vector2(moveDir * enemyData.moveSpeed, rigid.linearVelocity.y);
//
//        float dis = transform.position.x - startPos.x;
//        // Mathf.Abs : 절댓값 반환 (양수 음수 안가리고 이동한 거리 비교를 위해 사용
//        if (Mathf.Abs(dis) >= enemyData.moveDistance)
//        {
//            isMovingRight = !isMovingRight;
//            startPos = transform.position;
//            spriteRenderer.flipX = !spriteRenderer.flipX;
//        }
//    }
//
//    public override void Attack()
//    {
//        StartFire();
//    }
//
//    public override void Chase()
//    {
//        if (targetPlayer == null) return;
//
//        // 시야에 다시 들어오면 공격 모드로 전환
//        if (sensor != null && sensor.IsPlayerDetected)
//        {
//            isAttackMode = true;
//            isChaseMode = false;
//            return;
//        }
//
//        float dir = Mathf.Sign(targetPlayer.position.x - transform.position.x);
//
//        rigid.linearVelocity = new Vector2(dir * enemyData.moveSpeed, rigid.linearVelocity.y);
//
//        spriteRenderer.flipX = dir < 0;
//    }
//    #endregion
//}
