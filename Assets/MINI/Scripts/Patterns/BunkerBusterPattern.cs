using UnityEngine;

[CreateAssetMenu(fileName = "BunkerBusterPattern", menuName = "Boss/Patterns/BunkerBuster")]
public class BunkerBusterPattern : BossPattern
{
    [Header("Settings")]
    [SerializeField] private float prepTime = 0.5f;         // 점프 전 준비 시간
    [SerializeField] private float jumpSpeed = 40f;         // 올라가는 속도
    [SerializeField] private float jumpHeight = 15f;        // 화면 밖으로 나갈 높이 (Y축 거리)

    
    [SerializeField] private GameObject warningPrefab;      // 바닥에 깔릴 경고 장판
    [SerializeField] private float aimingDuration = 2.0f;   // 플레이어를 따라다니는 시간
    [SerializeField] private float lockOnTime = 0.5f;       // 피할 시간

    
    [SerializeField] private float dropSpeed = 60f;         // 낙하 속도
    [SerializeField] private LayerMask groundLayer;         // 바닥 감지용

    [SerializeField] private HitBoxStat explosionStat;      // 폭발 데미지 정보
    [SerializeField] private Vector2 explosionSize = new(8f, 3f); // 충격파 범위
    [SerializeField] private float recoveryTime = 1.5f;     // 착지 후 후딜레이

    // 캐싱용 변수
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCol;
    private GameObject currentWarning;

    public override void Initialize(BossAI boss)
    {
        base.Initialize(boss);
        spriteRenderer = boss.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = boss.GetComponentInChildren<SpriteRenderer>();
        bossCol = boss.GetComponent<Collider2D>();

        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Terrain", "Ground");
    }

    protected override async Awaitable Execute()
    {
        if (boss == null) return;
    
        // 준비
        animator.SetTrigger("Prep"); // 웅크리는 모션 등
        rb.linearVelocity = Vector2.zero;

        try { await Awaitable.WaitForSecondsAsync(prepTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }
                
        // Jump
       
        animator.SetTrigger("Jump");

        // 물리 엔진으로 상승
        rb.linearVelocity = Vector2.up * jumpSpeed;

        // 목표 높이까지 대기
        float startY = boss.transform.position.y;
        while (boss.transform.position.y < startY + jumpHeight)
        {
            await Awaitable.FixedUpdateAsync(boss.DestroyCancellationToken);
        }

        // 모습 숨기기 & 물리 끄기
        SetBossVisible(false);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f; // 고정
                     
        // 조준 Tracking
        
        Vector3 targetPos = boss.player.position;
        targetPos.y = GetGroundY(targetPos); // 바닥 높이 찾기

        if (warningPrefab != null)
        {
            currentWarning = Instantiate(warningPrefab, targetPos, Quaternion.identity);
        }

        float timer = 0f;

        // 플레이어 따라다니기
        while (timer < aimingDuration)
        {
            timer += Time.deltaTime;

            // 플레이어 이동
            Vector3 followPos = boss.player.position;
            followPos.y = GetGroundY(followPos);

            // 따라가기 
            if (currentWarning != null)
            {
                currentWarning.transform.position = Vector3.Lerp(currentWarning.transform.position, followPos, Time.deltaTime * 10f);
            }

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }

        // 피할 시간
        try { await Awaitable.WaitForSecondsAsync(lockOnTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 순간이동
        Vector3 dropPos = boss.transform.position;
        if (currentWarning != null) dropPos.x = currentWarning.transform.position.x;
        boss.transform.position = dropPos;

        // 낙하 시작
        SetBossVisible(true);
        rb.gravityScale = 1f; 
        rb.linearVelocity = Vector2.down * dropSpeed;

        // 낙하 모션
        animator.SetTrigger("Fall");

        // 바닥에 닿을 때까지 대기      
        while (true)
        {
            // 발밑 감지
            float checkDist = 1.0f;
            if (dropSpeed > 50f) checkDist = 3.0f; // 속도가 빠르면 더 멀리서 감지

            RaycastHit2D hit = Physics2D.Raycast(boss.transform.position, Vector2.down, checkDist, groundLayer);
            if (hit.collider != null)
            {
                Vector3 landPos = boss.transform.position;
                landPos.y = hit.point.y + (bossCol != null ? bossCol.bounds.extents.y : 0);
                boss.transform.position = landPos;
                break;
            }
            await Awaitable.FixedUpdateAsync(boss.DestroyCancellationToken);
        }
        
        // 충격      
        rb.linearVelocity = Vector2.zero;

        // 착지 모션     
        animator.SetTrigger("Land");

        // 경고 장판 삭제
        if (currentWarning != null) Destroy(currentWarning);

        // 광역 데미지 판정
        CheckExplosionDamage();

        // 후딜레이    
        try { await Awaitable.WaitForSecondsAsync(recoveryTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        boss.OnAnimationTrigger("AttackEnd");
    }
    private void SetBossVisible(bool isVisible)
    {
        // Boss 하위 렌더러들 끄고 켬
        if (spriteRenderer != null) spriteRenderer.enabled = isVisible;
        if (bossCol != null) bossCol.enabled = isVisible;                
    }

    private float GetGroundY(Vector3 pos)
    {
        // 하늘에서 바닥으로 레이를 쏴서 Y값 찾기
        RaycastHit2D hit = Physics2D.Raycast(pos + Vector3.up * 10f, Vector2.down, 20f, groundLayer);
        if (hit.collider != null) return hit.point.y;
        return pos.y; // 바닥 못 찾으면 그냥 현재 Y 사용
    }

    private void CheckExplosionDamage()
    {
        if (explosionStat == null) return;

        // 보스 발밑 중심
        Vector2 center = (Vector2)boss.transform.position + new Vector2(0, -0.5f);

        // 디버깅
        DebugDrawBox(center, explosionSize, Color.red, 1.0f);

        // 범위 내 적 타격
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, explosionSize, 0f, explosionStat.attackable);
        foreach (var hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(explosionStat.damage);
        }
    }

    private void DebugDrawBox(Vector2 center, Vector2 size, Color color, float duration)
    {
        Vector2 half = size * 0.5f;
        Vector2 p1 = center + new Vector2(-half.x, -half.y);
        Vector2 p2 = center + new Vector2(-half.x, half.y);
        Vector2 p3 = center + new Vector2(half.x, half.y);
        Vector2 p4 = center + new Vector2(half.x, -half.y);
        Debug.DrawLine(p1, p2, color, duration);
        Debug.DrawLine(p2, p3, color, duration);
        Debug.DrawLine(p3, p4, color, duration);
        Debug.DrawLine(p4, p1, color, duration);
    }

    public override void UpdatePattern() { }

    public override void ExitPattern()
    {
        //안전장치
        SetBossVisible(true);
        if (boss != null && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1f;
        }
        if (currentWarning != null) Destroy(currentWarning);
    }

    public override void OnAnimationEvent(string eventName) { }
}