using UnityEngine;

[CreateAssetMenu(fileName = "JabStraightPattern", menuName = "Boss/Patterns/JabStraight")]
public class JabStraightPattern : BossPattern
{    
    [SerializeField] private float prepTime = 0.5f;

    [Header("Jab")]
    [SerializeField] private float jabDuration = 0.5f;                      // 잽 총 시전시간
    [SerializeField] private float jabImpactTime = 0.2f;                    // 선딜
    [SerializeField] private float jabStepSpeed = 5f;                       // 잽 전진속도

    [SerializeField] private HitBoxStat jabStat;                            // HitBox SO
        
    [SerializeField] private float jabWidth = 1.5f;                         // 잽의 가로 길이
    [SerializeField] private Vector2 jabOffset = new(1.0f, 0f);             // 잽 대강 위치

    [Header("Straight")]
    [SerializeField] private float straightDuration = 0.8f;                 // 스트 총 시전시간
    [SerializeField] private float straightImpactTime = 0.4f;               // 딜
    [SerializeField] private float straightStepSpeed = 8f;                  // 잽 전진속도

    [SerializeField] private HitBoxStat straightStat;                       // 스트 SO

    [SerializeField] private float straightWidth = 2.5f;                    // 스트 가로 길이
    [SerializeField] private Vector2 straightOffset = new(1.0f, 0f);        // 스트 대강 위치

    
    [SerializeField] private float recoveryTime = 1.0f;                     // 후딜

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
        animator.SetTrigger("PatternPrepare");      // 이름 변경할 수도?
        boss.FaceTarget(boss.player.position);

        try { await Awaitable.WaitForSecondsAsync(prepTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 잽
        animator.SetTrigger("DoJab");
        MoveBoss(jabStepSpeed);

        try { await Awaitable.WaitForSecondsAsync(jabImpactTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }
                
        CheckHitBox(jabOffset, jabWidth, jabStat);

        float remainJabTime = Mathf.Max(0, jabDuration - jabImpactTime);
        try { await Awaitable.WaitForSecondsAsync(remainJabTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        rb.linearVelocity = Vector2.zero;

        // 스트레이트
        animator.SetTrigger("DoStraight");
        boss.FaceTarget(boss.player.position);
        MoveBoss(straightStepSpeed);

        try { await Awaitable.WaitForSecondsAsync(straightImpactTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        
        CheckHitBox(straightOffset, straightWidth, straightStat);

        float remainStraightTime = Mathf.Max(0, straightDuration - straightImpactTime);
        try { await Awaitable.WaitForSecondsAsync(remainStraightTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        rb.linearVelocity = Vector2.zero;

        // 후딜
        try { await Awaitable.WaitForSecondsAsync(recoveryTime, boss.DestroyCancellationToken); }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        boss.OnAnimationTrigger("AttackEnd");
    }

    private void MoveBoss(float speed)
    {
        float facingDir = Mathf.Sign(boss.transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDir * speed, 0f);
    }    
    private void CheckHitBox(Vector2 offset, float width, HitBoxStat stats)
    {
        if (stats == null)
        {
            Debug.LogError("Stat할당안됨.");
            return;
        }

        float facingDir = Mathf.Sign(boss.transform.localScale.x);
        Vector2 actualOffset = new(offset.x * facingDir, offset.y);
        Vector2 centerPos = (Vector2)boss.transform.position + actualOffset;

        float autoHeight = bossCol != null ? bossCol.bounds.size.y : 2.0f;
        Vector2 boxSize = new(width, autoHeight);

        DebugDrawBox(centerPos, boxSize, Color.red, 0.5f);
                
        Collider2D[] hits = Physics2D.OverlapBoxAll(centerPos, boxSize, 0f, stats.attackable);

        foreach (var hit in hits)
        {           
            GameObject target = hit.gameObject;

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {               
                damageable.TakeDamage(stats.damage);
                Debug.Log($"[Hit] {target.name}에게 {stats.damage} 데미지!");
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

    public override void OnAnimationEvent(string eventName) { }
}