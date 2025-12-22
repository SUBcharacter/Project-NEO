using UnityEngine;


[CreateAssetMenu(fileName = "JabStraightPattern_Event", menuName = "Boss/Patterns/JabStraight_EventVer")]
public class JabStraightPattern_Event : BossPattern
{
    [Header("1. Preparation")]
    [SerializeField] private float prepTime = 0.5f;

    [Header("2. Jab Settings")]    
    [SerializeField] private float jabStepDuration = 0.1f;  // 스텝 밟는 시간
    [SerializeField] private float jabStepSpeed = 15f;      // 스텝 속도
    [SerializeField] private float jabTotalDuration = 0.5f; // 전체 동작 시간

    [SerializeField] private HitBoxStat jabStat;
    [SerializeField] private float jabWidth = 1.5f;
    [SerializeField] private Vector2 jabOffset = new Vector2(1.0f, 0f);

    [Header("3. Straight Settings")]   
    [SerializeField] private float straightStepDuration = 0.15f;
    [SerializeField] private float straightStepSpeed = 20f;
    [SerializeField] private float straightTotalDuration = 0.8f;

    [SerializeField] private HitBoxStat straightStat;
    [SerializeField] private float straightWidth = 2.5f;
    [SerializeField] private Vector2 straightOffset = new Vector2(1.5f, 0f);

    [Header("4. Recovery")]
    [SerializeField] private float recoveryTime = 1.0f;

    // 캐싱 변수
    private Collider2D bossCol;
    private int targetLayer;

    public override void Initialize(BossAI boss)
    {
        base.Initialize(boss);
        bossCol = boss.GetComponent<Collider2D>();
        targetLayer = LayerMask.GetMask("Player");
    }

    protected override async Awaitable Execute()
    {
        if (boss == null) return;

        // 전조
        animator.SetTrigger("JabPrep");
        boss.FaceTarget(boss.player.position);
        try { await Awaitable.WaitForSecondsAsync(prepTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

       
        // 잽        
        animator.SetTrigger("DoJab");

        // 스텝         
        await StepMove(jabStepSpeed, jabStepDuration);

        // 남은 시간 대기
        float remainJab = Mathf.Max(0, jabTotalDuration - jabStepDuration);
        try { await Awaitable.WaitForSecondsAsync(remainJab, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 스트레이트
        animator.SetTrigger("DoStraight");
        boss.FaceTarget(boss.player.position);

        // 스텝
        await StepMove(straightStepSpeed, straightStepDuration);

        // 남은 시간 대기
        float remainStraight = Mathf.Max(0, straightTotalDuration - straightStepDuration);
        try { await Awaitable.WaitForSecondsAsync(remainStraight, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 후딜
        try { await Awaitable.WaitForSecondsAsync(recoveryTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        boss.OnAnimationTrigger("AttackEnd");
    }

    // 이동 로직 
    private async Awaitable StepMove(float speed, float duration)
    {
        float facingDir = Mathf.Sign(boss.transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDir * speed, 0f);

        try { await Awaitable.WaitForSecondsAsync(duration, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { throw; }

        rb.linearVelocity = Vector2.zero;
    }

    public override void OnAnimationEvent(string eventName)
    {
        // BossAI에서 호출할 "JabHit"
        if (eventName == "JabHit")
        {
            CheckHitBox(jabOffset, jabWidth, jabStat);
        }
        // BossAI에서 호출할 "StraightHit"
        else if (eventName == "StraightHit")
        {
            CheckHitBox(straightOffset, straightWidth, straightStat);
        }
    }

    
    // HitBox 판정 로직     
    private void CheckHitBox(Vector2 offset, float width, HitBoxStat stats)
    {
        if (stats == null) return;

        float facingDir = Mathf.Sign(boss.transform.localScale.x);
        Vector2 actualOffset = new Vector2(offset.x * facingDir, offset.y);
        Vector2 centerPos = (Vector2)boss.transform.position + actualOffset;

        float autoHeight = bossCol != null ? bossCol.bounds.size.y : 2.0f;
        Vector2 boxSize = new Vector2(width, autoHeight);

        // 디버깅
        DebugDrawBox(centerPos, boxSize, Color.red, 0.2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(centerPos, boxSize, 0f, stats.attackable);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(stats.damage);
                Debug.Log($"[Event Triggered] {hit.name}에게 {stats.damage} 데미지!");
            }
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
}