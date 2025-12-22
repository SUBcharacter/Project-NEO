using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// 분기 설정 제대로 하려고 만든건데...tq 샤이샤이한 보스가 되버렷네.... 패턴 수정도 못햇음....
public enum DistanceDecision
{
    Retreat,   // 너무 가까움 → Sway
    Approach,  // 너무 멂 → Dash
    Attack     // 적정 거리
}


public class TutoIdleBattleState : BossIdleState
{
    float timer;

    public TutoIdleBattleState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        timer = 0f;
        boss.rb.linearVelocity = Vector2.zero;
        boss.animator.SetTrigger("Idle");

        // 패턴만 미리 선택
        BossPattern pattern = boss.SelectBestPattern();
        boss.SetCurrentPattern(pattern);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer < boss.CurrentPhase.restTime) return;

        // 판단은 Sway/Dash에게 넘김
        boss.ChangeState(new TutoSwayState(boss));
    }
}

public class TutoSwayState : BossState
{
    float speed = 7f;
    Vector2 dir;

    public TutoSwayState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.animator.SetTrigger("Sway");
        boss.FaceTarget(boss.player.position);

        float dx = boss.player.position.x - boss.transform.position.x;
        dir = dx > 0 ? Vector2.left : Vector2.right; // 후퇴
    }

    public override void Update()
    {
        switch (boss.DecideDistance())
        {
            case DistanceDecision.Attack:
                boss.ChangeState(new TutoBossAttackingState(boss));
                return;

            case DistanceDecision.Approach:
                boss.ChangeState(new TutoDashState(boss));
                return;

            case DistanceDecision.Retreat:
                // 계속 후퇴
                boss.rb.linearVelocity = dir * speed;
                break;
        }
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }
}

public class TutoDashState : BossState
{
    float speed;
    Vector2 dir;

    public TutoDashState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.animator.SetTrigger("Dash");
        boss.FaceTarget(boss.player.position);

        float dx = boss.player.position.x - boss.transform.position.x;
        dir = dx > 0 ? Vector2.right : Vector2.left;

        speed = boss.CurrentPhase.speedMultiplier * 6f;
    }

    public override void Update()
    {
        switch (boss.DecideDistance())
        {
            case DistanceDecision.Attack:
                boss.ChangeState(new TutoBossAttackingState(boss));
                return;

            case DistanceDecision.Retreat:
                boss.ChangeState(new TutoSwayState(boss));
                return;

            case DistanceDecision.Approach:
                // 계속 접근
                boss.rb.linearVelocity = dir * speed;
                break;
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

    public TutoBossAttackingState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.rb.linearVelocity = Vector2.zero;
        boss.Attacking = true;

        currentPattern = boss.CurrentPattern;
        if (currentPattern == null)
        {
            boss.ChangeState(new TutoIdleBattleState(boss));
            return;
        }

        currentPattern.Initialize(boss);
        currentPattern.StartPattern();

        boss.animator.SetTrigger("Attack");
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




//public class TutoBossHitState : BossState
//{
//    float timer;

//    public TutoBossHitState(BossAI boss) : base(boss) { }

//    public override void Start()
//    {
//        timer = 0f;
//        boss.rb.linearVelocity = Vector2.zero;
//        boss.animator.SetTrigger("Hit");
//    }

//    public override void Update()
//    {
//        timer += Time.deltaTime;

//        if (timer >= 0.3f)
//        {
//            boss.ChangeState(boss.previousState ?? new TutoIdleBattleState(boss));
//        }
//    }

//    public override void Exit()
//    {

//    }
//}