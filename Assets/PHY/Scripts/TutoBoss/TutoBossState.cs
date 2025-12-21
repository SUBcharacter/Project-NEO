using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// ==========================
//   TUTO IDLE (대기)
// ==========================
public class TutoIdleBattleState : BossIdleState
{
    float timer;

    public TutoIdleBattleState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        timer = 0f;
        boss.animator.SetTrigger("Idle");
        boss.rb.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= boss.CurrentPhase.restTime)
        {
            if (Random.value < 0.5f)
                boss.ChangeState(new TutoSwayState(boss));
            else
                boss.ChangeState(new TutoDashState(boss));
        }
    }
}





public class TutoSwayState : BossState
{
    float speed;
    Vector2 dir;

    public TutoSwayState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.animator.SetTrigger("Sway");

        bool faceRight = boss.transform.localScale.x > 0;
        dir = faceRight ? Vector2.left : Vector2.right;

        speed = 4f;
    }

    public override void Update()
    {
        speed = Mathf.Lerp(speed, 0f, 3f * Time.deltaTime);
        boss.rb.linearVelocity = dir * speed;

        float dist = Vector2.Distance(boss.transform.position, boss.player.position);

        // ← 여기서 BossPattern 기반 공격으로 진입
        if (dist <= 3f)
        {
            boss.ChangeState(new TutoBossAttackingState(boss));
            return;
        }

        if (speed <= 0.01f)
            boss.ChangeState(new TutoIdleBattleState(boss));
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }
}




// ==========================
//   TUTO DASH (근접 돌진)
// ==========================
public class TutoDashState : BossState
{
    float speed;
    Vector2 dir;

    public TutoDashState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.animator.SetTrigger("Dash");

        bool facingRight = boss.transform.localScale.x > 0;
        dir = facingRight ? Vector2.right : Vector2.left;

        speed = boss.CurrentPhase.speedMultiplier * 5f;
    }

    public override void Update()
    {
        boss.rb.linearVelocity = dir * speed;

        float dist = Vector2.Distance(boss.transform.position, boss.player.position);

        if (dist < 3f)
        {
            boss.ChangeState(new TutoBossAttackingState(boss));
        }
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }
}



public class TutoBossAttackingState : BossState
{
    private BossPattern currentPattern;
    private float timer;

    public TutoBossAttackingState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        boss.rb.linearVelocity = Vector2.zero;
        boss.Attacking = true;                   // 팀장이 말한 bool 분기
        timer = 0f;

        //  튜토보스 전용 패턴 가져오기
        currentPattern = boss.SelectBestPattern();

        // 또는 튜토보스 전용 패턴 배열에서 직접 꺼내기
        // currentPattern = boss.TutoPatterns[0];

        if (currentPattern != null)
        {
            currentPattern.Initialize(boss);     // 필수
            currentPattern.StartPattern();       // 필수
        }

        boss.animator.SetTrigger("Attack");
    }

    public override void Update()
    {
        if (currentPattern == null) return;

        // 패턴 내부 구조 그대로 사용
        currentPattern.UpdatePattern();

        // 패턴 끝났는지 체크
        if (currentPattern.IsFinished)
        {
            boss.Attacking = false;
            boss.ChangeState(new TutoIdleBattleState(boss));
        }
    }

    public override void OnAnimationEvent(string eventName)
    {
        currentPattern?.OnAnimationEvent(eventName);
    }

    public override void Exit()
    {
        currentPattern?.ExitPattern();
        boss.Attacking = false;
    }
}




public class TutoBossHitState : BossState
{
    float timer;

    public TutoBossHitState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        timer = 0f;
        boss.rb.linearVelocity = Vector2.zero;
        boss.animator.SetTrigger("Hit");
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0.3f)
        {
            boss.ChangeState(boss.previousState);
        }
    }

    public override void Exit() { }
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
