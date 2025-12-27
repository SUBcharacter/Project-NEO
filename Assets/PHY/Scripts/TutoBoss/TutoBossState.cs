using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
/// 2025-12-23
/// TODO: 팀장 피드백 후 추가 수정 및 보스패턴+기본공격 작업 예정
/// </summary>


// #TODO
// 1. BossAI에 플레이어와 보스의 X축 거리(float)를 반환하는 public 함수를 작성 -> 완료
// 2. BossPattern을 상속한 패턴들의 Execute함수 초반에 거리 판단 로직을 작성 -> 완료
// 3. 상태 전환 흐름 재정비 -> 의도대로 작동하는지 확인필요
// ※ 모르는 것이 있다면 언제든 알릴 것!

// BossIdleState를 상속 받는게 아닌 추상 클래스인 BossState를 상속 받아야함
public class TutoIdleBattleState : BossState // 작업 완
{
    BossPattern currentPattern;

    float timer;

    public TutoIdleBattleState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        timer = 0f;
        boss.rb.linearVelocity = Vector2.zero;
        boss.animator.SetTrigger("Idle");

        // 패턴만 미리 선택
        currentPattern = boss.SelectBestPattern();
        boss.SetCurrentPattern(currentPattern);
    }

    public override void Update()
    {
        // restTime이 끝나면 AttackingState로 넘기기

        timer += Time.deltaTime;
        if (timer < boss.CurrentPhase.restTime) return;

        boss.ChangeState(new TutoBossAttackingState(boss, currentPattern));
    }

    public override void Exit()
    {
        
    }
}

public class TutoSwayState : BossState
{
    BossPattern currentPattern;

    Vector2 dir;

    // 상태패턴이 안정되면 SO로 뺄 예정
    float speed = 30f; // 이 속도는 플레이어와 동일한 속도 => // 플레이어 이동 속도 기준으로 조정 예정 7
    float decelSpeed = 3f;   // 감속 속도 (중간에서 컷 해주는거 인듯) 3
    float stop = 0.5f;      // 0.5

    // 스웨이 연출법
    // - 진행할 방향으로 X축 속도를 적정하게 준다 ex. rigid.linearVelocityX = direction(float 아니면 Vector2.x) * 30f
    // - 현재 속도를 담아둘 speed 변수(이건 완료, 다만 Start에서 linearVelocity(현재속도)를 넣어줘야함) 생성 및 초기화
    // - Mathf.Lerp를 이용해서 최종 타겟을 0으로 잡고, speed에 지속적으로 업데이트 시켜준후 linearVelocityX에다 덮어씌우기
    // - 해당 프레임에 마지막으로 산출된 speed를 기준으로 상한선 설정(Lerp 특성상 설정한 타겟에는 도달하기 힘들기에, 중간에서 컷 해줘야한다)
    // - 속도 업데이트가 중단됨과 동시에 다시 AttackingState로 전환

    public TutoSwayState(BossAI boss, BossPattern _currentPattern) : base(boss) { currentPattern = _currentPattern; }

    public override void Start()
    {
        boss.animator.SetTrigger("Sway");
        boss.FaceTarget(boss.player.position);

        float dx = boss.player.position.x - boss.transform.position.x;
        dir = dx > 0 ? Vector2.left : Vector2.right; // 후퇴

        speed = Mathf.Abs(boss.rb.linearVelocity.x);
        if (speed < 0.01f)
            speed = 30f;
    }

    public override void Update()
    {
        // 거리 판단은 CurrentPattern에 담겨 있는 Execute에서 진행
        // 스웨이가 끝나면 바로 Attacking 상태로 넘겨버리면 됨
        // 거리판단 로직은 필요하나 현재 사용되고 있는 DecideDistance로는 한계가 있음
        // 스웨이 로직은 BisiliState의 BSSwayState.Update를 참고 할 것

        speed = Mathf.Lerp(speed, 0, Time.deltaTime * decelSpeed);

        boss.rb.linearVelocityX = dir.x * speed;

        if(speed <= stop)
        {
            boss.rb.linearVelocity = Vector2.zero;
            boss.ChangeState(new TutoBossAttackingState(boss, currentPattern));
        }

    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }
}

public class TutoDashState : BossState
{
    BossPattern currentPattern;

    // 상태패턴이 안정되면 SO로 뺄 예정
    float speed;
    float decelSpeed = 4f;      // 4
    float stop = 0.5f;          // 0.5

    Vector2 dir;

    // 대쉬 연출법
    // - 진행할 방향으로 X축 속도를 적정하게 준다 ex. rigid.linearVelocityX = direction(float 아니면 Vector2.x) * 30f
    // - 현재 속도를 담아둘 speed 변수(이건 완료, 다만 Start에서 linearVelocity(현재속도)를 넣어줘야함) 생성 및 초기화
    // - Mathf.Lerp를 이용해서 최종 타겟을 0으로 잡고, speed에 지속적으로 업데이트 시켜준후 linearVelocityX에다 덮어씌우기
    // - 해당 프레임에 마지막으로 산출된 speed를 기준으로 상한선 설정(Lerp 특성상 설정한 타겟에는 도달하기 힘들기에, 중간에서 컷 해줘야한다)
    // - 속도 업데이트가 중단됨과 동시에 다시 AttackingState로 전환

    public TutoDashState(BossAI boss, BossPattern _currentPattern) : base(boss) { currentPattern = _currentPattern; }

    public override void Start()
    {
        boss.animator.SetTrigger("Dash");
        boss.FaceTarget(boss.player.position);

        float dx = boss.player.position.x - boss.transform.position.x;
        dir = dx > 0 ? Vector2.right : Vector2.left;

        speed = boss.CurrentPhase.speedMultiplier * 30f;     // TODO: 플레이어 이동 속도 기준으로 튜닝이 필요할진 모르지만 일단 보류

    }

    public override void Update()
    {
        // 거리 판단은 CurrentPattern에 담겨 있는 Execute에서 진행
        // 스웨이가 끝나면 바로 Attacking 상태로 넘겨버리면 됨
        // 거리판단 로직은 필요하나 현재 사용되고 있는 DecideDistance로는 한계가 있음
        // 대쉬 로직은 Player.Dodge와 PlayerDodgeState.Update를 참고 할 것

        speed = Mathf.Lerp(speed, 0, Time.deltaTime * decelSpeed);

        boss.rb.linearVelocityX = dir.x * speed;

        if(Mathf.Abs(speed) <= stop)
        {
            boss.rb.linearVelocity = Vector2.zero;

            boss.ChangeState(new TutoBossAttackingState(boss, currentPattern));
        }
        
        
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }
}

public class TutoBossAttackingState : BossState
{
    BossPattern currentPattern;

    // 중요! 현재 거리 판단에 따른 Sway, Dash 상태전환과 큰 연관이 있음!
    // 현재 Idle에서 뽑았던 패턴을 BossAI에 만들어둔 CurrentState에다 넣어서 참조 하는방식
    // 문제가 있는 것은 아니지만 다른 방법으로도 옮길 수 있음

    // 각 스테이트 별 생성자에 매개 변수로 Idle에서 뽑았던 패턴을 넘겨 주면 됨. 이 부분은 작업 해 두겠음
    // ex. TutoBossAttackingState(BossAI boss, BossPattern currentPattern) : base(boss) { this.currentPattern = currentPattern; }

    // AttackingState 진입 시 Initialize를 통해 뽑아놓은 패턴이 BossAI를 인식하고, 이를 통해 BossAI에 있는 거리 산출 함수를 사용할 수 있음
    // StartPattern 실행시 Execute 초반 BossAI에 있는 거리 판단 함수를 이용해서 사거리 판단을 하고,
    // 각 상황에 맞게 'Execute에서' Sway 혹은 Dash로 ChangeState를 호출 할 것임.
    // Sway, Dash는 행동이 끝나면 바로 다시 AttackingState로 돌아올 것이고,
    // Initialize와 StartPattern을 거쳐서 패턴이 실행 될 경우, 패턴이 끝났다면 다시 Idle로 돌려보내면 됨
    // Idle에 진입하면 새로운 패턴을 뽑을 것이기 때문에 문제 없음.
    // 거리 판단 로직은 현재 만들어둔 패턴에 가이드를 작성 해둘 것이니 참고 할 것
    
    public TutoBossAttackingState(BossAI boss, BossPattern currentPattern) : base(boss) { this.currentPattern = currentPattern; }

    public override void Start()
    {
        boss.rb.linearVelocity = Vector2.zero;
        boss.Attacking = true;

        if (currentPattern == null)
        {
            boss.ChangeState(new TutoIdleBattleState(boss));
            return;
        }

        currentPattern.Initialize(boss);
        currentPattern.StartPattern();

    }

    public override void Update()
    {
        currentPattern.UpdatePattern();

        if (currentPattern.IsFinished)
        {
            boss.Attacking = false;
            boss.ChangeState(new TutoIdleBattleState(boss));
        }
    }

    public override void OnAnimationEvent(string eventName)
    {
        Debug.Log($"[State] Event: {eventName}, pattern = {currentPattern}");
        currentPattern.OnAnimationEvent(eventName);
    }

    public override void Exit()
    {
       
        boss.Attacking = false;
    }
}

public class TutoBossDeathState : BossState
{
    public TutoBossDeathState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.rb.linearVelocity = Vector2.zero;
        boss.animator.SetTrigger("Death");
    }

    public override void Update() { }

    public override void Exit() { }
}
