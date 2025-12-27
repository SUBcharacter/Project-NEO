using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "GrabPattern", menuName = "Boss/Patterns/Grab")]
public class GrabPattern : BossPattern
{
    [SerializeField] float dashSpeed = 13f;
    [SerializeField] float dashDuration = 1.0f;
    [SerializeField] Vector3 liftOffset = new(0.5f, 0.8f, 0f);
    [SerializeField] private float patternDamage = 40f;

    // 캐싱
    [NonSerialized] private Rigidbody2D targetRb;       // RealPlayer의 RB (물리 끄기용)
    [NonSerialized] private Transform targetRoot;       // detectedPlayer(껍데기) (이동/납치용)
    [NonSerialized] private PlayerMove targetMoveScript; // PlayerMove 스크립트 (이동 잠금용)
    [NonSerialized] bool isCaught = false;


    // 잡는 위치 

    public void Initialize()
    {
        base.Initialize(boss);
        if (boss == null)
        {
            Debug.LogError("GrabPattern의 boss참조가 Null임");
            return;
        }
        isCaught = false;

        if (boss.player != null)
        {
            targetRoot = boss.player;
            targetRb = targetRoot.GetComponent<Rigidbody2D>();
            targetMoveScript = targetRoot.GetComponent<PlayerMove>();
        }
        // 안전장치: 타겟이 없으면 패턴 취소
        if (targetRb == null || targetRoot == null)
        {
            ExitPattern();
            return;
        }
    }
    protected override async Awaitable Execute()
    {
        Initialize();
        if (targetRoot == null) return;

        // 1. 전조
        animator.SetTrigger("ReadyGrab");
        boss.FaceTarget(boss.player.position);

        try
        {
            await Awaitable.WaitForSecondsAsync(1f, boss.PatternCancellationToken);
        }
        catch (OperationCanceledException) { ExitPattern(); return; }

        // 2. 돌진 (이제 여기선 충돌만 감지하면 됨)
        await DashingAndSearching();

        rb.linearVelocity = Vector2.zero;

        // 3. 분기
        if (isCaught)
        {
            await ProcessGrabSequence();
        }
        else
        {
            animator.SetTrigger("GrabMiss");
            try
            {
                await Awaitable.WaitForSecondsAsync(1.0f, boss.PatternCancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        boss.OnAnimationTrigger("AttackEnd");
    }

    private async Awaitable DashingAndSearching()
    {
        animator.SetTrigger("DoDash");

        // RealPlayer를 향해 돌진
        Vector2 dir = (boss.player.position - boss.transform.position).normalized;
        dir.y = 0;

        rb.linearVelocity = dir * dashSpeed;

        float t = 0f;
        int playerLayer = LayerMask.GetMask("Player");

        while (t < dashDuration)
        {
            t += Time.deltaTime;

            Vector2 checkBoxPos = (Vector2)boss.transform.position + (dir * 1.5f);
            Collider2D hit = Physics2D.OverlapBox(checkBoxPos, new Vector2(2f, 2f), 0, playerLayer);

            if (hit != null)
            {
                if (hit.transform == targetRoot || hit.transform.IsChildOf(targetRoot))
                {
                    isCaught = true;
                    break;
                }
            }
            await Awaitable.NextFrameAsync(boss.PatternCancellationToken);
        }
    }

    // targetRb 건드리지 않고 스테이트 접근으로다가
    // Grab 패턴 에서 ChangingState함수에 stats["Stun"] 뭐 이런식으로 접근

    private async Awaitable ProcessGrabSequence()
    {
        // 1. 제어권 박탈
        if (targetRb)
        {
            targetRb.linearVelocity = Vector2.zero;
            targetRb.simulated = false; // 물리 끔
        }
        if (targetMoveScript) targetMoveScript.enabled = false; // 이동 끔

        // 2. 위치 강제 동기화 (납치)
        targetRoot.position = boss.transform.position;
        targetRoot.SetParent(boss.transform);

        // 초기화
        targetRoot.rotation = Quaternion.identity;
        // 납치 시 스케일이 꼬이지 않도록 보정 (필요시)
        // targetRoot.localScale = Vector3.one; 

        // 3. 들어 올리기
        animator.SetTrigger("DoLift");

        float liftTime = 0f;
        Vector3 startLocalPos = targetRoot.localPosition;
        Vector3 targetLocalPos = liftOffset; // 로컬 좌표계 사용

        while (liftTime < 0.3f)
        {
            liftTime += Time.deltaTime;
            targetRoot.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, liftTime / 0.3f);
            await Awaitable.NextFrameAsync(boss.PatternCancellationToken);
        }

        try
        {
            await Awaitable.WaitForSecondsAsync(0.2f, boss.PatternCancellationToken);
        }
        catch (OperationCanceledException) { ReleasePlayer(); return; } // 취소되면 놔주기

        // 4. 내다 꽂기
        animator.SetTrigger("DoSlam");

        float slamTime = 0f;
        Vector3 peakLocalPos = targetRoot.localPosition;
        Vector3 groundLocalPos = new(1.5f, -0.5f, 0); // 보스 발밑 앞쪽

        while (slamTime < 0.1f)
        {
            slamTime += Time.deltaTime;
            float t = slamTime / 0.1f;
            targetRoot.localPosition = Vector3.Lerp(peakLocalPos, groundLocalPos, t * t); // 가속 느낌
            await Awaitable.NextFrameAsync(boss.PatternCancellationToken);
        }

        // 데미지 입히기
        IDamageable victim = targetRoot.GetComponent<IDamageable>();
        victim?.TakeDamage(patternDamage);

        // 5. 해방
        ReleasePlayer();
        await Awaitable.WaitForSecondsAsync(0.5f, boss.PatternCancellationToken);
    }
    private void ReleasePlayer()
    {
        if (targetRoot != null)
        {
            targetRoot.SetParent(null);
            targetRoot.rotation = Quaternion.identity;
            targetRoot.localScale = Vector3.one; // 스케일 원복
        }

        if (targetRb) targetRb.simulated = true;
        if (targetMoveScript) targetMoveScript.enabled = true;
    }

    public override void UpdatePattern() { }

    public override void ExitPattern()
    {
        ReleasePlayer();
        isCaught = false;
    }

    public override void OnAnimationEvent(string eventName) { }
}