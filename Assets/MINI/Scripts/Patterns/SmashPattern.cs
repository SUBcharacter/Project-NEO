using UnityEngine;

[CreateAssetMenu(fileName = "SmashPattern", menuName = "Boss/Patterns/Smash")]
public class SmashPattern : BossPattern
{
    [SerializeField] float patternDuration = 1.2f;
    [SerializeField] int repeatCount = 1;
    [SerializeField] float delayBetweenSmash = 0.4f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject smashFXPrefab;
    //[SerializeField] AnimationCurve jumpCurve; // 높이 조절용 커브 혹시몰라서 생성

    [SerializeField] float jumpHeight = 2.0f;

    Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        float x = Mathf.Lerp(start.x, end.x, t);
        float y = Mathf.Lerp(start.y, end.y, t) + Mathf.Sin(t * Mathf.PI) * height;
        return new Vector2(x, y);
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
                try
                {
                    await Awaitable.WaitForSecondsAsync(delayBetweenSmash, boss.DestroyCancellationToken);
                }
                catch (System.OperationCanceledException) { return; }
            }
        }
        await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);

        boss.OnAnimationTrigger("AttackEnd");
    }

    private async Awaitable Smash()
    {
        animator.SetTrigger("ReadySmash");
        try
        {
            await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { return; }

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
        Vector2 targetPos = groundPos;
        targetPos.y += bossHalfHeight;

        float duration = patternDuration / 2f;        
        float t = 0f;

        while (t < 1f)
        {
            t += Time.fixedDeltaTime / duration;

            if (t > 1f) t = 1f;

            Vector2 nextPos = Parabola(start, targetPos, jumpHeight, t);

            rb.MovePosition(nextPos);

            await Awaitable.FixedUpdateAsync(boss.DestroyCancellationToken);
        }
        rb.MovePosition(targetPos);

        rb.linearVelocity = Vector2.zero;

        boss.animator.SetBool("IsSmashing", false);

        if (smashFXPrefab != null)
            Instantiate(smashFXPrefab, groundPos + new Vector3(0f, 0.5f, 0f), Quaternion.identity);


    }
    Vector2 GetGroundPosition(Vector3 targetPos)
    {
        // 디버깅용 시각화
        Vector2 rayStart = (Vector2)targetPos + Vector2.up * 5f;
        Debug.DrawRay(rayStart, Vector3.down * 15f, Color.green, 2.0f);

        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 15f, groundLayer);

        if (hit.collider != null)
        {
            return new Vector2(targetPos.x, hit.point.y);
        }
        else
        {
            Debug.LogWarning("바닥없음");
            return new Vector2(targetPos.x, targetPos.y - 1.0f);
        }
    }
    public override void OnAnimationEvent(string eventName) { }
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
