using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;

public class TutoBossState : BossState
{
    protected TutoBossAI tutoAI;
    protected bool isRequestChange = false;
    protected float tutoBossSpeed = 3f;
    public TutoBossState(BossAI boss) : base(boss)
    {
        tutoAI = boss as TutoBossAI;
    }

    public override void Start() { }

    public override void Update() { }

    public void RequestChange(TutoBossState tutoBossNext)
    {
        if (isRequestChange) return;
        isRequestChange = true;
        boss.ChangeState(tutoBossNext);

    }
    public override void Exit()
    {
        isRequestChange = false;
    }

}

public class TutoIdleState : TutoBossState
{
    private bool isPatrolling = false;
    private float distanceToPlayer = 99f;

    public TutoIdleState(BossAI boss) : base(boss) { }

    public override async void Start()
    {
        isPatrolling = true;
        Debug.Log("Idle 시작됨");
        // Idle 애니메이션
        boss.animator.SetTrigger("Idle");

        // 패트롤 시작 
        _ = PatrolRoutine();    // await 없이 하는 이유가 뭐지
    }

    private async Task PatrolRoutine()
    {
        Vector3 leftPos = boss.transform.position + Vector3.left * 2f;
        Vector3 rightPos = boss.transform.position + Vector3.right * 3f;

        while (isPatrolling)
        {
            // 왼쪽 패트롤
            boss.FaceTarget(leftPos);
            await MoveTo(leftPos);
            if (!isPatrolling) return;
            await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);

            // 오른쪽 패트롤
            boss.FaceTarget(rightPos);
            await MoveTo(rightPos);
            if (!isPatrolling) return;
            await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);
        }
    }

    private async Task MoveTo(Vector3 targetPos)
    {
        while (isPatrolling && Vector2.Distance(boss.transform.position, targetPos) > 0.05f)
        {
            boss.transform.position = Vector3.MoveTowards
                (boss.transform.position, targetPos,
                tutoBossSpeed * Time.deltaTime);

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }
    }



    public override void Update()
    {
        if (boss.player == null) return;

        float dist = Mathf.Abs(boss.player.position.x - boss.transform.position.x);
        //Debug.Log($"[Idle] Update — PlayerDist: {dist}");

        // Chase 전환 조건
        if (dist < distanceToPlayer)
        {
            Debug.Log("[Idle → Chase] 플레이어 접근함, 상태 전환");
            isPatrolling = false;   // 패트롤 즉시 종료
            RequestChange(new TutoChaseState(boss));
        }
    }

    public override void Exit()
    {
        // 상태 변경시 반드시 패트롤 종료
        isPatrolling = false;
        base.Exit();
    }
}



public class TutoChaseState : TutoBossState
{
    private float chaseSpeed = 2.5f;

    // 패턴 평가 간격
    private float patternCheckInterval = 0.7f;
    private float patternCheckTimer = 0f;

    private TutoBossAI tuto;

    public TutoChaseState(BossAI boss) : base(boss)
    {
        tuto = boss as TutoBossAI;
    }

    public override void Start()
    {
        Debug.Log("Idle -> Walk로 넘어감");
        boss.animator.SetTrigger("Walk");
    }

    public override void Update()
    {
        if (boss.player == null) return;

        // 1) 플레이어 추적
        TrackPlayer();

        // 2) 일정 텀마다 패턴들 점수 평가
        patternCheckTimer += Time.deltaTime;

        if (patternCheckTimer >= patternCheckInterval)
        {
            patternCheckTimer = 0f;

            if (HasExecutablePattern())
            {
                Debug.Log("[Chase → Attack] 실행 가능한 패턴 발견, Attack 진입");
                RequestChange(new TutoAttackingState(boss));
            }
        }
    }

    private void TrackPlayer()
    {
        float targetX = boss.player.position.x;
        float bossY = boss.transform.position.y;

        Vector2 targetPos = new Vector3(targetX, bossY, boss.transform.position.z);

        boss.FaceTarget(boss.player.position);

        boss.transform.position = Vector3.MoveTowards(
            boss.transform.position,
            targetPos,
            chaseSpeed * Time.deltaTime
        );
    }

    private bool HasExecutablePattern()
    {
        // 1) 튜토리얼 패턴 체크 (순서 방식)
        if (tuto.tutorialIndex < tuto.tutorialSequence.Count)
        {
            var p = tuto.tutorialSequence[tuto.tutorialIndex];
            if (p.EvaluateScore(boss) > 0)
                return true;
        }

        // 2) 랜덤 패턴 체크
        foreach (var p in tuto.randomPatterns)
        {
            if (p.EvaluateScore(boss) > 0)
                return true;
        }

        return false;
    }

    public override void Exit() { }
}


public class TutoAttackingState : TutoBossState
{
    private BossPattern currentPattern;
    private TutoBossAI tuto;

    public TutoAttackingState(BossAI boss) : base(boss)
    {
        tuto = tutoAI;
    }


    public override void Start()
    {
        currentPattern = SelectPattern();
        Debug.Log($"[Attack] Start — 선택된 패턴: {currentPattern}");

        if (currentPattern == null)
        {
            Debug.Log("[Attack] 패턴 없음 → CoolDown으로 전환");
            RequestChange(new TutoCoolDownState(boss, 1f));
            return;
        }

        currentPattern.Initialize(boss);
        Debug.Log("[Attack] 패턴 StartPattern 실행");
        currentPattern.StartPattern();
    }


    private BossPattern SelectPattern()
    {
        // 1) 튜토리얼 순서 우선
        if (tuto.tutorialIndex < tuto.tutorialSequence.Count)
        {
            BossPattern p = tuto.tutorialSequence[tuto.tutorialIndex];
            float score = p.EvaluateScore(boss);

            if (score > 0) // 조건 맞아야 실행
            {
                tuto.tutorialIndex++;
                return p;
            }
        }

        // 2) 랜덤 패턴 — 조건 맞는 패턴만 후보에 넣기
        List<BossPattern> validPatterns = new List<BossPattern>();

        foreach (var p in tuto.randomPatterns)
        {
            float score = p.EvaluateScore(boss);
            if (score > 0)
                validPatterns.Add(p);
        }

        // 조건 맞는 패턴이 1개도 없는 경우
        if (validPatterns.Count == 0)
            return null;

        // 3) 가중치 기반 선택
        float total = 0f;
        foreach (var p in validPatterns)
            total += p.EvaluateScore(boss);

        float rand = Random.Range(0, total);
        float sum = 0f;

        foreach (var p in validPatterns)
        {
            sum += p.EvaluateScore(boss);
            if (rand <= sum)
                return p;
        }

        return validPatterns[0];
    }



    public override void Update()
    {
        currentPattern?.UpdatePattern();
    }

    public override void OnAnimationEvent(string eventName)
    {
        Debug.Log($"[Attack] AnimationEvent 받음 → {eventName}");

        if (eventName == "AttackEnd")
        {
            Debug.Log("[Attack] AttackEnd 수신 → CoolDown 진입");
            currentPattern?.ExitPattern();
            RequestChange(new TutoCoolDownState(boss, 1f));
        }
        else
        {
            currentPattern?.OnAnimationEvent(eventName);
        }
    }

    public override void Exit()
    {
        currentPattern?.ExitPattern();
    }
}

public class TutoCoolDownState : TutoBossState
{
    private float timer;

    public TutoCoolDownState(BossAI boss, float coolTime) : base(boss)
    {
        timer = coolTime;
    }

    public override void Start()
    {
       
        boss.animator.SetTrigger("Idle");
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        //Debug.Log($"[CoolDown] Update — 남은 시간: {timer}");

        if (timer <= 0)
        {
            Debug.Log("[CoolDown → Chase] 쿨타임 종료, 추적 재개");
            RequestChange(new TutoChaseState(boss));
        }
    }

    public override void Exit() { }
}