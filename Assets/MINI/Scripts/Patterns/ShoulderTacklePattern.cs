using UnityEngine;

[CreateAssetMenu(fileName = "ShoulderTacklePattern", menuName = "Boss/Patterns/ShoulderTackle")]
public class ShoulderTacklePattern : BossPattern
{
    [Header("Time Settings")]
    [SerializeField] private float preludeDuration = 2.0f;      // 전조 시간
    [SerializeField] private float dashDuration = 1.0f;         // 돌진 시간
    [SerializeField] private float postDuration = 1.5f;         // 후딜레이 

    [Header("Physics Settings")]
    [SerializeField] private float maxSpeed = 25f;              // 순간 최대 속도

    [Header("Motion Curve")]
    [Tooltip("시간(0~1)에 따른 속도 그래프. \n추천: 시작(0)은 높게, 끝(1)은 0으로 떨어지게 설정")]
    [SerializeField] private AnimationCurve speedCurve;

    // [캐싱용]
    [System.NonSerialized] private GameObject tackleHitBox;

    public override void Initialize(BossAI boss)
    {
        base.Initialize(boss);

        // 재귀 탐색으로 HitBox 찾기
        Transform found = FindChildRecursive(boss.transform, "TackleHitBox");
        if (found != null)
        {
            tackleHitBox = found.gameObject;
            tackleHitBox.SetActive(false);
        }
    }

    // [중요] StartPattern이 아니라 Execute를 오버라이드!
    protected override async Awaitable Execute()
    {
        if (boss == null) return;
               
        // 전조
       
        animator.SetTrigger("ShoulderTackleReady");
        boss.FaceTarget(boss.player.position);

        try
        {
            await Awaitable.WaitForSecondsAsync(preludeDuration, boss.PatternCancellationToken);
        }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 돌진 
        
        if (tackleHitBox != null) tackleHitBox.SetActive(true);

        await Dash();

        if (tackleHitBox != null) tackleHitBox.SetActive(false);

        
        // 후딜
        
        rb.linearVelocity = Vector2.zero;

       
        // animator.SetTrigger("TackleEnd"); 또는 ("Idle");

        await Awaitable.WaitForSecondsAsync(postDuration, boss.PatternCancellationToken);

        // 종료
        boss.OnAnimationTrigger("AttackEnd");
    }

    async Awaitable Dash()
    {
        animator.SetTrigger("ShoulderTackleDash");

        // 방향 결정
        float directionX = Mathf.Sign(boss.player.position.x - boss.transform.position.x);
        Vector2 dashDir = new(directionX, 0);

        float t = 0f;

        // FixedUpdate
        while (t < 1f)
        {
            t += Time.fixedDeltaTime / dashDuration;
            if (t > 1f) t = 1f;

            // Curve에서 현재 시간의 속도 배율을 가져옴 // Phase에있는 애니메이션 속도 배율을 패턴 속도로 엮음.
            float speedMultiplier = (speedCurve.length > 0) ? speedCurve.Evaluate(t) : 1f;

            //Y축 속도 유지
            rb.linearVelocity = new Vector2(dashDir.x * maxSpeed * speedMultiplier, rb.linearVelocity.y);

            await Awaitable.FixedUpdateAsync(boss.PatternCancellationToken);
        }
        rb.linearVelocity = Vector2.zero;
    }

    public override void UpdatePattern() { }

    public override void ExitPattern()
    {
        if (tackleHitBox != null) tackleHitBox.SetActive(false);
        if (boss != null && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public override void OnAnimationEvent(string eventName) { }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}