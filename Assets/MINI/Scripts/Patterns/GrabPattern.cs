using System;
using UnityEngine;

public class GrabPattern : BossPattern
{
    float dashSpeed = 10f;
    float dashDuration = 1.0f;

    // 미리 찾아둘 변수들 (캐싱)
    private Rigidbody2D targetRb;       // RealPlayer의 RB (물리 끄기용)
    private Transform targetRoot;       // Player(껍데기) (이동/납치용)
    private MonoBehaviour targetMoveScript; // PlayerMove 스크립트 (이동 잠금용)

    bool isCaught = false;

    // 잡는 위치 
    Vector3 liftOffset = new Vector3(0.5f, 1.5f, 0f);

    public GrabPattern(BossAI boss) : base(boss) { }

    public void Initialize() 
    {
        isCaught = false;

        if (boss.player != null)
        {
            targetRb = boss.player.GetComponent<Rigidbody2D>();
            targetRoot = boss.player.parent;

            // Root에 있는 이동 스크립트 찾기 (이름은 실제 스크립트에 맞게 수정)
            if (targetRoot != null)
                targetMoveScript = targetRoot.GetComponent<PlayerMove>();
        }

        // 안전장치: 타겟이 없으면 패턴 취소
        if (targetRb == null || targetRoot == null)
        {
            Exit();
            return;
        }
    }
    public override async void Start()
    {
        Initialize();

        // 1. 전조
        animator.SetTrigger("ReadyGrab");
        LookAtPlayer();

        try
        {
            await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);
        }
        catch (OperationCanceledException) { Exit(); return; }

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
            await Awaitable.WaitForSecondsAsync(1.0f, boss.DestroyCancellationToken);
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
                isCaught = true;
                break;
            }

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }
    }

    private async Awaitable ProcessGrabSequence()
    {
        // 1. 미리 찾아둔 정보로 제어 시작
        if (targetRb) targetRb.simulated = false; // 물리 끔
        if (targetMoveScript) targetMoveScript.enabled = false; // 이동 키 입력 끔

        // 2. 부모(Root)를 납치 (카메라까지 딸려옴)
        targetRoot.SetParent(boss.transform);

        // 3. 들어 올리기
        animator.SetTrigger("DoLift");

        float liftTime = 0f;
        Vector3 startLocalPos = targetRoot.localPosition;

        float directionSign = Mathf.Sign(boss.transform.localScale.x);
        Vector3 targetLocalPos = new(liftOffset.x * directionSign, liftOffset.y, 0);

        while (liftTime < 0.3f)
        {
            liftTime += Time.deltaTime;
            targetRoot.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, liftTime / 0.3f);
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }

        await Awaitable.WaitForSecondsAsync(0.2f, boss.DestroyCancellationToken);

        // 4. 내다 꽂기
        animator.SetTrigger("DoSlam");

        float slamTime = 0f;
        Vector3 peakLocalPos = targetRoot.localPosition;
        Vector3 groundLocalPos = new Vector3(1.5f * directionSign, 0.5f, 0);

        while (slamTime < 0.1f)
        {
            slamTime += Time.deltaTime;
            float t = slamTime / 0.1f;
            targetRoot.localPosition = Vector3.Lerp(peakLocalPos, groundLocalPos, t * t);
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }

        // 5. 해방
        targetRoot.SetParent(null);
        targetRoot.rotation = Quaternion.identity; // 회전값 초기화

        if (targetRb) targetRb.simulated = true;
        if (targetMoveScript) targetMoveScript.enabled = true;

        await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);
    }

    void LookAtPlayer()
    {
        if (boss.player.position.x > boss.transform.position.x)
            boss.transform.localScale = new Vector3(1, 1, 1);
        else
            boss.transform.localScale = new Vector3(-1, 1, 1);
    }

    public override void Update() { }

    public override void Exit()
    {
        // 종료 시 안전하게 해제
        if (targetRoot != null)
        {
            targetRoot.SetParent(null);
            targetRoot.rotation = Quaternion.identity;
        }
        if (targetRb != null) targetRb.simulated = true;
        if (targetMoveScript != null) targetMoveScript.enabled = true;
    }

    public override void OnAnimationEvent(string eventName) { }
}