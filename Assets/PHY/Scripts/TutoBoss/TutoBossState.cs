using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;

///////////////////////////////////////////////////////////////
// [TutoBossState]
// - 모든 튜토보스 상태의 공통 부모
// - 상태 전환 요청 플래그, 이동속도 등 기본 기능 제공
///////////////////////////////////////////////////////////////
public class TutoBossState : BossState
{
    protected TutoBossAI tutoAI;
    protected bool isRequestChange = false;
    // 중복 상태 전환 방지용 플래그

    protected float tutoBossSpeed = 3f;
    // Idle 패트롤 이동에 사용됨 (상태별로 필요시 override 가능)

    public TutoBossState(BossAI boss) : base(boss)
    {
        tutoAI = boss as TutoBossAI;
    }

    public override void Start() { }
    public override void Update() { }

    // 상태 전환 요청 (중복 요청 방지)
    public void RequestChange(TutoBossState tutoBossNext)
    {
        if (isRequestChange) return;
        isRequestChange = true;
        boss.ChangeState(tutoBossNext);
    }

    public override void Exit()
    {
        // 다음 상태 진입 시 다시 전환 가능하도록 초기화
        isRequestChange = false;
    }
}


///////////////////////////////////////////////////////////////
// [TutoIdleState]
// - 등장 후 기본 대기 상태
// - 좌우 패트롤 + 플레이어 감지 후 Chase로 이동
///////////////////////////////////////////////////////////////
public class TutoIdleState : TutoBossState
{
    private bool isPatrolling = false;
    private float distanceToPlayer = 99f;
    // 플레이어 감지 거리 (임시값)

    public TutoIdleState(BossAI boss) : base(boss) { }

    public override async void Start()
    {
        isPatrolling = true;

        Debug.Log("Idle 시작됨");

        // Idle 애니메이션 재생
        boss.animator.SetTrigger("Idle");

        // 패트롤 시작
        // await을 붙지 않는 이유:
        // → 비동기 루틴이 끝날 때까지 대기하지 않고, Start 흐름은 그대로 진행
        _ = PatrolRoutine();
    }

    // 좌우로 자동 이동하는 루틴
    private async Task PatrolRoutine()
    {
        Vector3 leftPos = boss.transform.position + Vector3.left * 2f;
        Vector3 rightPos = boss.transform.position + Vector3.right * 3f;

        while (isPatrolling)
        {
            // 왼쪽 이동
            boss.FaceTarget(leftPos);
            await MoveTo(leftPos);
            if (!isPatrolling) return;

            await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);

            // 오른쪽 이동
            boss.FaceTarget(rightPos);
            await MoveTo(rightPos);
            if (!isPatrolling) return;

            await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);
        }
    }

    // 목표 좌표까지 이동 (프레임 단위 이동)
    private async Task MoveTo(Vector3 targetPos)
    {
        while (isPatrolling && Vector2.Distance(boss.transform.position, targetPos) > 0.05f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                targetPos,
                tutoBossSpeed * Time.deltaTime);

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }
    }

    public override void Update()
    {
        if (boss.player == null) return;

        float dist = Mathf.Abs(boss.player.position.x - boss.transform.position.x);

        // Chase 전환 조건
        if (dist < distanceToPlayer)
        {
            Debug.Log("[Idle → Chase] 플레이어 접근함, 상태 전환");
            isPatrolling = false;  // 패트롤 종료
            RequestChange(new TutoChaseState(boss));
        }
    }

    public override void Exit()
    {
        // 상태 전환 시 패트롤 강제 종료
        isPatrolling = false;
        base.Exit();
    }
}


///////////////////////////////////////////////////////////////
// [TutoChaseState]
// - 플레이어 좌우 위치 따라가며 추적
// - 일정 텀마다 패턴 실행 가능 여부 평가
// - 공격 간격 쿨타임도 여기서 관리
///////////////////////////////////////////////////////////////
public class TutoChaseState : TutoBossState
{
    private float chaseSpeed = 2.5f;

    // 패턴 평가 주기
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

        // 공격 쿨타임이 남았으면 → 무조건 추적만
        if (Time.time < tuto.lastAttackTime + tuto.minAttackInterval)
        {
            TrackPlayer();
            return;
        }

        // 추적
        TrackPlayer();

        // 패턴 체크 타이머
        patternCheckTimer += Time.deltaTime;

        // 일정 텀마다 패턴 평가
        if (patternCheckTimer >= patternCheckInterval)
        {
            patternCheckTimer = 0f;

            if (HasExecutablePattern())
            {
                Debug.Log("[Chase → Attack] 실행 가능한 패턴 발견");
                RequestChange(new TutoAttackingState(boss));
            }
        }
    }

    // 플레이어 X좌표로 따라가기
    private void TrackPlayer()
    {
        float targetX = boss.player.position.x;
        float bossY = boss.transform.position.y;

        Vector3 targetPos = new Vector3(targetX, bossY, boss.transform.position.z);

        boss.FaceTarget(boss.player.position);

        boss.transform.position = Vector3.MoveTowards(
            boss.transform.position,
            targetPos,
            chaseSpeed * Time.deltaTime);
    }

    // 실행 가능한 패턴 있나 평가
    private bool HasExecutablePattern()
    {
        // 1) 튜토리얼 패턴 (순서 고정)
        if (tuto.tutorialIndex < tuto.tutorialSequence.Count)
        {
            var p = tuto.tutorialSequence[tuto.tutorialIndex];
            if (p.EvaluateScore(boss) > 0)
                return true;
        }

        // 2) 랜덤 패턴
        foreach (var p in tuto.randomPatterns)
        {
            if (p.EvaluateScore(boss) > 0)
                return true;
        }

        return false;
    }

    public override void Exit() { }
}


///////////////////////////////////////////////////////////////
// [TutoAttackingState]
// - 패턴 선정 → 실행 → 애니메이션 이벤트 종료 → CoolDown
///////////////////////////////////////////////////////////////
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
        // 실행할 패턴 선택
        currentPattern = SelectPattern();
        Debug.Log($"[Attack] Start — 선택된 패턴: {currentPattern}");

        if (currentPattern == null)
        {
            // 조건 맞는 패턴 하나도 없음
            Debug.Log("[Attack] 패턴 없음 → CoolDown");
            RequestChange(new TutoCoolDownState(boss, 1f));
            return;
        }

        // 패턴 실행 준비
        currentPattern.Initialize(boss);

        Debug.Log("[Attack] StartPattern 실행");
        currentPattern.StartPattern();
    }

    // 패턴 선택 로직
    private BossPattern SelectPattern()
    {
        // 1) 튜토 순서 우선
        if (tuto.tutorialIndex < tuto.tutorialSequence.Count)
        {
            BossPattern p = tuto.tutorialSequence[tuto.tutorialIndex];
            if (p.EvaluateScore(boss) > 0)
            {
                tuto.tutorialIndex++;
                return p;
            }
        }

        // 2) 랜덤 패턴 중 조건 맞는 것만 후보로 가져오기
        List<BossPattern> valid = new List<BossPattern>();

        foreach (var p in tuto.randomPatterns)
        {
            if (p.EvaluateScore(boss) > 0)
                valid.Add(p);
        }

        if (valid.Count == 0)
            return null;

        // 3) 가중치 기반 랜덤 선택
        float total = 0;
        foreach (var p in valid) total += p.EvaluateScore(boss);

        float rand = Random.Range(0, total);
        float sum = 0;

        foreach (var p in valid)
        {
            sum += p.EvaluateScore(boss);
            if (rand <= sum)
                return p;
        }

        return valid[0];
    }

    public override void Update()
    {
        // 패턴 진행 중 호출
        currentPattern?.UpdatePattern();
    }

    public override void OnAnimationEvent(string eventName)
    {
        Debug.Log($"[Attack] AnimationEvent → {eventName}");

        if (eventName == "AttackEnd")
        {
            // 공격 쿨타임 기록
            tuto.lastAttackTime = Time.time;

            Debug.Log("[Attack] AttackEnd → CoolDown");
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


///////////////////////////////////////////////////////////////
// [TutoCoolDownState]
// - 공격 종료 후 잠시 대기 상태
// - 일정 시간 지나면 Chase로 복귀
///////////////////////////////////////////////////////////////
public class TutoCoolDownState : TutoBossState
{
    private float timer;

    public TutoCoolDownState(BossAI boss, float coolTime) : base(boss)
    {
        timer = coolTime;    // CoolDown 지속 시간
    }

    public override void Start()
    {
        // Idle 포즈 유지
        boss.animator.SetTrigger("Idle");
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Debug.Log("[CoolDown → Chase] 쿨타임 종료");
            RequestChange(new TutoChaseState(boss));
        }
    }

    public override void Exit() { }
}
