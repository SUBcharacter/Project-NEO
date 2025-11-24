using UnityEngine;

public class SmashPattern : BossPattern
{
    float beforeYOffSet;
    float patternDuration = 1.2f;

    public SmashPattern(BossAI boss) : base(boss) { }

    Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Vector3 pos = Vector3.Lerp(start, end, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * height;
        return pos;
    }
    public override async void Start()
    {
        animator.SetTrigger("ReadySmash");
        beforeYOffSet = boss.transform.position.y;
        try
        {
            await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            Exit();
            return;
        }

        animator.SetTrigger("DoSmash");
        animator.SetBool("IsSmashing", true);

        Vector3 start = boss.transform.position;
        Vector3 target = GetAdjustedLandingPosition(boss.player.position);

        var rb = boss.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        float duration = patternDuration / 2f;
        float height = 2f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (t > 1f) t = 1f;

            boss.transform.position = Parabola(start, target, height, t);


            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }
        boss.transform.position = target;
        rb.linearVelocity = Vector2.zero;

        boss.animator.SetBool("IsSmashing", false);
        boss.OnAnimationTrigger("AttackEnd");
    }

    Vector3 GetAdjustedLandingPosition(Vector3 playerPosition)
    {
        int groundLayer = LayerMask.GetMask("Ground");
        if (groundLayer == 0) Debug.LogError("Ground 없음.");

        RaycastHit2D hit = Physics2D.Raycast(playerPosition + Vector3.up * 2f, Vector2.down, 10f, groundLayer);

        Debug.DrawRay(playerPosition + Vector3.up * 2f, Vector2.down * 10f, Color.red, 2.0f);

        Vector3 landPos = playerPosition;
        if (hit.collider != null)
        {
            landPos.y = hit.point.y;
        }
        else
        {
            // 바닥을 못 찾음 -> 현재 플레이어 Y좌표 사용 (임시 방편)            
            Debug.LogWarning("오류임");
        }

        Collider2D bossCollider = boss.GetComponent<Collider2D>();
        if (bossCollider != null)
        {
            landPos.y += bossCollider.bounds.extents.y; // 스프라이트 생기면 조정 필요함
        }
        landPos.x = playerPosition.x;

        return landPos;
    }


    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "SmashImpact")
        {
            
        }
    }


    // update 에다가 이제 쫒아가는 로직 짜야함 애니메이션 trigger 쏴줬고 transform.position 이런거로 플레이어 위치로 이동하게
    public override void Update()
    {
        //animator.SetBool("IsSmashing", false); <- 이거 나중에 애니메이션 클립에서 처리하거나 해야할 듯?
        /* 여따가 뭐 유도 로직이나 기타 작성*/
    }

    public override void Exit()
    {
        // boss.OnAnimationTrigger("AttackEnd");
        // boss.ChangeState(new IdleState(boss));
    }
}
