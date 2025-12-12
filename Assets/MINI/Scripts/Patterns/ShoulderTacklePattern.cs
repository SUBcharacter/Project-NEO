using UnityEngine;

[CreateAssetMenu(fileName = "ShoulderTacklePattern", menuName = "Boss/Patterns/ShoulderTackle")]
public class ShoulderTacklePattern : BossPattern
{
    [Header("Settings")]
    [SerializeField] private float preludeDuration = 1f;        // 전조 시간
    [SerializeField] private float tackleDistance = 10f;        // 돌진 거리
    [SerializeField] private float dashDuration = 1.2f;         // 돌진하는데 걸리는 총 시간 (짧을수록 빠름)

    [Header("Motion")]    
    [SerializeField] private AnimationCurve movementCurve;      // 로그 함수 느낌을 내기 위한 그래프

    
    private GameObject tackleHitBox; // 보스한테 붙어있는 히트박스 캐싱용

    protected override async Awaitable Execute()
    {
        if (boss == null) return;
               
        if (tackleHitBox == null)
        {
            Transform t = boss.transform.Find("TackleHitBox");
            if (t != null) tackleHitBox = t.gameObject;
        }

        // 전조 
        animator.SetTrigger("ShoulderTackleReady");

        // 차징ing
        boss.FaceTarget(boss.player.position);

        // 전조 시간 대기
        try
        {
            await Awaitable.WaitForSecondsAsync(preludeDuration, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            ExitPattern();
            return;
        }

        // 돌진 
        if (tackleHitBox != null) tackleHitBox.SetActive(true);

        await Dash(); // Dash가 끝날 때까지 기다림

        if (tackleHitBox != null) tackleHitBox.SetActive(false);
               
        // 후딜
        await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);

        boss.OnAnimationTrigger("AttackEnd");
    }

    async Awaitable Dash()
    {
        animator.SetTrigger("ShoulderTackleDash");
                
        Vector3 startPos = boss.transform.position;                     // 시작 위치
        Vector2 dir = (boss.player.position - boss.transform.position).normalized;      // 방향 벡터
             
        dir.y = 0f; // 수평 돌진만 하도록 Y 성분 제거
        Vector2 targetPos = startPos + (Vector3)(dir * tackleDistance);

        // 이거 고려해보는 거 좋을 수도 생각해보기 <---- !!!
        //RaycastHit2D raycastHit = Physics2D.Raycast(startPos, dir, tackleDistance, LayerMask.GetMask("Ground"));
                
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dashDuration;
            if (t > 1f) t = 1f;
            
            // 기획하시는 분이 원하시는 대로
            // Curve를 "처음엔 급격하게 올라가다가 나중에 평평해지는" 모양으로 그리면 됨
            float curveValue = (movementCurve.length > 0) ? movementCurve.Evaluate(t) : t;

            // Lerp로 위치 이동 (Start -> Target)
            boss.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }
        // 보정       
        boss.transform.position = targetPos;
    }
    // 플레이어 방향 구하기
    

    public override void UpdatePattern() { }

    public override void ExitPattern()
    {
        // 패턴 캔슬되면 히트박스 꺼줘야 함
        if (tackleHitBox != null) tackleHitBox.SetActive(false);
    }

    public override void OnAnimationEvent(string eventName) { }
}