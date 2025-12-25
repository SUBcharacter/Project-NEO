using UnityEngine;

[CreateAssetMenu(fileName = "JabStraightPattern", menuName = "Boss/Patterns/JabStraight")]
public class JabStraightPattern : BossPattern
{
    [Header("Pre")]
    [SerializeField] private float prepTime = 0.5f;

    [Header("Jab")]
    [Tooltip("이동 시간")]
    [SerializeField] private float jabStepDuration = 0.1f;
    [Tooltip("전진 속도")]
    [SerializeField] private float jabStepSpeed = 15f;
    [Tooltip("총 대기 시간")]
    [SerializeField] private float jabImpactTime = 0.25f;
    [SerializeField] private float jabTotalDuration = 0.5f; // 전체 모션 시간

    [SerializeField] private HitBoxStat jabStat;
    [SerializeField] private float jabWidth = 1.5f;
    [SerializeField] private Vector2 jabOffset = new(1.0f, 0f);

    [Header("Straight")]
    [Tooltip("스텝 시간")]
    [SerializeField] private float straightStepDuration = 0.15f;
    [SerializeField] private float straightStepSpeed = 20f;
    [SerializeField] private float straightImpactTime = 0.4f;
    [SerializeField] private float straightTotalDuration = 0.8f;

    [SerializeField] private HitBoxStat straightStat;
    [SerializeField] private float straightWidth = 2.5f;
    [SerializeField] private Vector2 straightOffset = new(1.5f, 0f);

    [Header("Recovery")]
    [SerializeField] private float recoveryTime = 1.0f; // 후딜

    // 캐싱 변수
    private Collider2D bossCol;

    public override void Initialize(BossAI boss)
    {
        base.Initialize(boss);
        bossCol = boss.GetComponent<Collider2D>();
    }

    protected override async Awaitable Execute()
    {
        if (boss == null) return;

        // 전조
        animator.SetTrigger("JabPrep");
        boss.FaceTarget(boss.player.position);
        try { await Awaitable.WaitForSecondsAsync(prepTime, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }
                
        // 잽 실행        
        animator.SetTrigger("DoJab");        
        await StepAndPunch(jabStepSpeed, jabStepDuration, jabImpactTime, jabStat, jabWidth, jabOffset);

        // 대기
        float remainJab = Mathf.Max(0, jabTotalDuration - jabImpactTime);
        try { await Awaitable.WaitForSecondsAsync(remainJab, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }
                
        // 스트레이트 실행        
        animator.SetTrigger("DoStraight");
        boss.FaceTarget(boss.player.position);                
        await StepAndPunch(straightStepSpeed, straightStepDuration, straightImpactTime, straightStat, straightWidth, straightOffset);

        // 남은 시간 대기
        float remainStraight = Mathf.Max(0, straightTotalDuration - straightImpactTime);
        try { await Awaitable.WaitForSecondsAsync(remainStraight, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }
        
        // 후딜
        try { await Awaitable.WaitForSecondsAsync(recoveryTime, boss.PatternCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        boss.OnAnimationTrigger("AttackEnd");
    }
        
    private async Awaitable StepAndPunch(float speed, float stepTime, float impactTime, HitBoxStat stat, float width, Vector2 offset)
    {
        float facingDir = Mathf.Sign(boss.transform.localScale.x);

        //  전진!
        rb.linearVelocity = new Vector2(facingDir * speed, 0f);

        try
        {
            // 시간만큼만 이동
            await Awaitable.WaitForSecondsAsync(stepTime, boss.PatternCancellationToken);
        }
        catch (System.OperationCanceledException) { throw; }

        // 급정지 
        rb.linearVelocity = Vector2.zero;
        // 타격 판정
        CheckHitBox(offset, width, stat);

        // 타격 대기 (이미 스텝 시간만큼 지났으니, 남은 시간만 대기)
        float remainWait = Mathf.Max(0, impactTime - stepTime);

        if (remainWait > 0)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(remainWait, boss.PatternCancellationToken);
            }
            catch (System.OperationCanceledException) { throw; }
        }

    }

    private void CheckHitBox(Vector2 offset, float width, HitBoxStat stats)
    {
        if (stats == null) return;

        float facingDir = Mathf.Sign(boss.transform.localScale.x)+ Mathf.Sign(boss.transform.localScale.x);
        Vector2 actualOffset = new(offset.x * facingDir, offset.y);

        // 보스의 위치를 기준으로 박스 생성
        Vector2 centerPos = (Vector2)boss.transform.position + actualOffset;

        float autoHeight = bossCol != null ? bossCol.bounds.size.y : 2.0f;
        Vector2 boxSize = new(width, autoHeight);

        // 디버깅
        DebugDrawBox(centerPos, boxSize, Color.red, 0.2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(centerPos, boxSize, 0f, stats.attackable);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(stats.damage);
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
        if (boss != null && rb != null) rb.linearVelocity = Vector2.zero;
    }

    public override void OnAnimationEvent(string eventName) { }
}