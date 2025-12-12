using UnityEngine;

[CreateAssetMenu(fileName = "SmashPattern", menuName = "Boss/Patterns/Smash")]
public class SmashPattern : BossPattern
{
    [SerializeField] float beforeYOffSet;
    [SerializeField] float patternDuration = 1.2f;
    [SerializeField] int repeatCount = 1;
    [SerializeField] float delayBetweenSmash = 0.4f;
    [SerializeField] LayerMask groundLayer;
    //[SerializeField] AnimationCurve jumpCurve; // 높이 조절용 커브 혹시몰라서 생성

    [SerializeField] GameObject smashFXPrefab;
    private Vector3 smashPrefaboffset;

    Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Vector3 pos = Vector3.Lerp(start, end, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * height;
        return pos;
    }
    protected override async Awaitable Execute()
    {
        if (boss == null)
        {
            Debug.LogError("SmashPattern의 boss참조가 Null임");
            return;
        }
        if (groundLayer == 0)       
            groundLayer = LayerMask.GetMask("Default", "Terrain", "Ground");

        for (int i = 0; i < repeatCount; i++)
        {
            await Smash();
            if (i < repeatCount - 1)
            {
                await Awaitable.WaitForSecondsAsync(delayBetweenSmash, boss.DestroyCancellationToken);
            }
        }
        boss.OnAnimationTrigger("AttackEnd");
    }

    private async Awaitable Smash()
    {
        animator.SetTrigger("ReadySmash");
        beforeYOffSet = boss.transform.position.y;
        try
        {
            await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            ExitPattern();
            return;
        }

        animator.SetTrigger("DoSmash");
        animator.SetBool("IsSmashing", true);

        Vector3 start = boss.transform.position;
        Vector3 groundPos = GetGroundPosition(boss.player.position);

        float bossHalfHeight = 0f;
        Collider2D bossCol = boss.GetComponent<Collider2D>();

        if (bossCol != null)
        {
            bossHalfHeight = bossCol.bounds.extents.y;
        }
        Vector3 bossTargetPos = groundPos;
        bossTargetPos.y += bossHalfHeight;

        float duration = patternDuration / 2f;
        float height = 2f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (t > 1f) t = 1f;

            boss.transform.position = Parabola(start, bossTargetPos, height, t);

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }
        boss.transform.position = bossTargetPos;
        rb.linearVelocity = Vector2.zero;

        boss.animator.SetBool("IsSmashing", false);
        if (smashFXPrefab != null)
            Instantiate(smashFXPrefab, groundPos + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
            
        
    }
    Vector3 GetGroundPosition(Vector3 targetPos)
    {
        // 디버깅용 시각화
        Vector3 rayStart = targetPos + Vector3.up * 5f;
        Debug.DrawRay(rayStart, Vector3.down * 15f, Color.green, 2.0f);

        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 15f, groundLayer);

        Vector3 result = targetPos;

        if (hit.collider != null)
        {
            result.y = hit.point.y;
            //Debug.Log($"Smash 바닥 감지 성공: {hit.collider.name} / Y: {result.y}");
        }
        else
        {
            Debug.LogWarning("Smash 바닥 감지 실패! (레이어 확인 필요). 보스 현재 Y축으로 대체합니다.");
            
            // 일단은 '플레이어의 발 밑'이라 가정하고 조금 내림
            result.y = targetPos.y - 1.0f;
        }

        result.x = targetPos.x;
        return result;
    }
    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "SmashImpact")
        {

        }
    }  
    public override void UpdatePattern()
    {
        //animator.SetBool("IsSmashing", false); <- 이거 나중에 애니메이션 클립에서 처리하거나 해야할 듯?
        /* 여따가 뭐 유도 로직이나 기타 작성*/
    }

    public override void ExitPattern()
    {
        // boss.OnAnimationTrigger("AttackEnd");
        // boss.ChangeState(new IdleState(boss));
    }
}
