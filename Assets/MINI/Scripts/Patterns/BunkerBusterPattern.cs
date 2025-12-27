using UnityEngine;

[CreateAssetMenu(fileName = "BunkerBusterPattern", menuName = "Boss/Patterns/BunkerBuster")]
public class BunkerBusterPattern : BossPattern
{
    [Header("Jump")]
    [SerializeField] private float prepTime = 0.5f;
    [SerializeField] private float jumpSpeed = 40f;

    [Tooltip("천장이 없을 때 최대 상승 높이")]
    [SerializeField] private float maxJumpHeight = 20f;
    [Tooltip("천장 감지 거리 (이 거리만큼 남기고 멈춤)")]
    [SerializeField] private float ceilingStopDist = 3.0f;

    [Header("Targeting")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float trackingTime = 1.0f;     // 따라다니는 시간
    [SerializeField] private float lockOnTime = 0.5f;       // 멈춰있는 시간

    [Header("Drop")]
    [SerializeField] private float dropSpeed = 60f;
    [SerializeField] private LayerMask groundLayer;         // 천장 겸 바닥 레이어

    [Header("Impact")]
    [SerializeField] private HitBoxStat explosionStat;
    [SerializeField] private Vector2 explosionSize = new(8f, 3f);
    [SerializeField] private float recoveryTime = 1.5f;

    // 캐싱용
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

        // 원상복구용 백업
        float originalGravity = rb.gravityScale;
        bool originalTrigger = bossCol != null ? bossCol.isTrigger : false;

       
        // 준비
        
        animator.SetTrigger("Prep");
        boss.FaceTarget(boss.player.position);
        rb.linearVelocity = Vector2.zero;

        try { await Awaitable.WaitForSecondsAsync(prepTime, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }


        
        // 상승 (천장 감지)       
        animator.SetTrigger("Jump");

        // 올라갈 때는 물리 충돌 끄고 중력 끄고 올라감
        if (bossCol != null) bossCol.isTrigger = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.up * jumpSpeed;

        float startY = boss.transform.position.y;

        // 천장에 머리 박기 직전까지만 상승
        while (true)
        {
            RaycastHit2D hit = Physics2D.Raycast(boss.transform.position, Vector2.up, ceilingStopDist + 1.0f, groundLayer);

            // 천장이 감지되었고, 3f보다 가까워지면 정지
            if (hit.collider != null && hit.distance <= ceilingStopDist)
            {
                break;
            }
            // 혹은 최대 높이까지 올라갔으면 정지
            if (boss.transform.position.y >= startY + maxJumpHeight)
            {
                break;
            }
            await Awaitable.FixedUpdateAsync(boss.PatternCancellationToken);
        }

        // 정지 및 숨기기
        rb.linearVelocity = Vector2.zero;
        SetBossVisible(false);
        
        // 트래킹        
        float hoverY = boss.transform.position.y;

        // 경고 장판 생성
        Vector3 warnPos = boss.player.position;
        warnPos.y = GetGroundY(warnPos);
        if (warningPrefab != null)
            currentWarning = Instantiate(warningPrefab, warnPos, Quaternion.identity);

        float timer = 0f;
        while (timer < trackingTime)
        {
            timer += Time.deltaTime;

            float targetX = boss.player.position.x;
            
            // 보스가 천장 속에 파묻히지 않게 아까 멈춘 높이(hoverY) 유지
            boss.transform.position = new Vector3(targetX, hoverY, 0f);

            // 경고 장판 이동
            if (currentWarning != null)
            {
                Vector3 followPos = new Vector3(targetX, GetGroundY(new Vector3(targetX, hoverY, 0)), 0);
                // 부드럽게 따라가기
                currentWarning.transform.position = Vector3.Lerp(currentWarning.transform.position, followPos, Time.deltaTime * 15f);
            }

            await Awaitable.NextFrameAsync(boss.PatternCancellationToken);
        }

        // 락온         
        try { await Awaitable.WaitForSecondsAsync(lockOnTime, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 낙하   
        if (currentWarning != null)
        {
            Vector3 finalPos = boss.transform.position;
            finalPos.x = currentWarning.transform.position.x;
            boss.transform.position = finalPos;
        }
        SetBossVisible(true);
        animator.SetTrigger("Fall");

        rb.linearVelocity = Vector2.down * dropSpeed;

        // 바닥 감지
        while (true)
        {
            float checkDist = (dropSpeed * Time.fixedDeltaTime) + 2.0f;
            RaycastHit2D hit = Physics2D.Raycast(boss.transform.position, Vector2.down, checkDist, groundLayer);

            if (hit.collider != null)
            {
                Vector3 landPos = boss.transform.position;
                landPos.y = hit.point.y + (bossCol != null ? bossCol.bounds.extents.y : 0);
                boss.transform.position = landPos;
                break;
            }
            await Awaitable.FixedUpdateAsync(boss.PatternCancellationToken);
        }
        // 충격 및 종료
        rb.linearVelocity = Vector2.zero;

        // 물리 상태 복구
        if (bossCol != null) bossCol.isTrigger = originalTrigger;
        rb.gravityScale = originalGravity;

        animator.SetTrigger("Land");
        //CameraShake.instance.Shake(0.8f, 5f);

        if (currentWarning != null) Destroy(currentWarning);
        CheckExplosionDamage();

        try { await Awaitable.WaitForSecondsAsync(recoveryTime, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        boss.OnAnimationTrigger("AttackEnd");
    }
    private void SetBossVisible(bool isVisible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = isVisible;
    }

    private float GetGroundY(Vector3 pos)
    {
        Vector2 origin = pos + Vector3.up * 1.0f;

        // ContactFilter로 'Trigger'는 무시하도록 설정
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false; 

        RaycastHit2D[] results = new RaycastHit2D[1];
        int hitCount = Physics2D.Raycast(origin, Vector2.down, filter, results, 20f);

        if (hitCount > 0)
        {
            return results[0].point.y;
        }
        // 바닥 못 찾으면 플레이어 발밑(현재 Y) 리턴
        return pos.y;
    }

    private void CheckExplosionDamage()
    {
        if (explosionStat == null) return;
        Vector2 center = (Vector2)boss.transform.position + new Vector2(0, -0.5f);
        DebugDrawBox(center, explosionSize, Color.red, 1.0f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, explosionSize, 0f, explosionStat.attackable);
        foreach (var hit in hits) hit.GetComponent<IDamageable>()?.TakeDamage(explosionStat.damage);
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
        SetBossVisible(true);
        if (boss != null)
        {
            if (boss.GetComponent<Collider2D>()) boss.GetComponent<Collider2D>().isTrigger = false;
            if (rb != null) { rb.linearVelocity = Vector2.zero; rb.gravityScale = 1f; }
        }
        if (currentWarning != null) Destroy(currentWarning);
    }

    public override void OnAnimationEvent(string eventName) { }
}