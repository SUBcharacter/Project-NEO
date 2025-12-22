using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TutoIdleBattleState : BossIdleState
{
    float timer;

    // 거리 기준값 (튜닝 포인트)
    // SO로 빼야하나? 따로 거리체크가 있나? 패턴에 있는 Min Max Range로 한다했나..? 
    float close = 2f;
    float swayRange = 6f;
    float dashRange = 10f;

    public TutoIdleBattleState(BossAI boss) : base(boss) { }

  
    public override void Start()
    {
        timer =0f;
        boss.animator.SetTrigger("Idle");
        boss.rb.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        
        timer += Time.deltaTime;
        if (timer < boss.CurrentPhase.restTime) return;

        float dist = Vector2.Distance(boss.transform.position, boss.player.position);

        // 너무 가까움 → 뒤로 빠지기
        if (dist < close)
        {
            boss.ChangeState(new TutoSwayState(boss, false));
            return;
        }

        // 너무 멂 → Dash
        if (dist > dashRange)
        {
            boss.ChangeState(new TutoDashState(boss));
            return;
        }

        // 너무 가까움 → Sway 접근
        if (dist > swayRange)
        {
            boss.ChangeState(new TutoSwayState(boss, true));
            return;
        }

        // 적절한 거리 → (나중에 Attack)
        
        boss.ChangeState(new TutoBossAttackingState(boss));
        // boss.ChangeState(new TutoBossAttackingState(boss));
    }

    public override void Exit() { }
    
}


public class TutoSwayState : BossState
{
    float speed;
    Vector2 dir;
    bool approach; // true = 접근, false = 후퇴

    public TutoSwayState(BossAI boss, bool _approach) : base(boss)
    {
        approach = _approach;
    }

    public override void Start()
    {
        boss.animator.SetTrigger("Sway");

        float dx = boss.player.position.x - boss.transform.position.x;

        // 방향 결정
        if (approach)
            dir = dx > 0 ? Vector2.right : Vector2.left;   // 플레이어 쪽
        else
            dir = dx > 0 ? Vector2.left : Vector2.right;   // 플레이어 반대

        boss.FaceTarget(boss.player.position);

        speed = 7f;
    }

    public override void Update()
    {
        speed = Mathf.Lerp(speed, 0f, 3f * Time.deltaTime);
        boss.rb.linearVelocity = dir * speed;

        // 이동 끝나면 다시 판단
        if (speed <= 0.05f)
        {
            boss.ChangeState(new TutoIdleBattleState(boss));
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

        float dx = boss.player.position.x - boss.transform.position.x;
        dir = dx > 0 ? Vector2.right : Vector2.left;
        boss.FaceTarget(boss.player.position);
        speed = boss.CurrentPhase.speedMultiplier * 6f;
    }

    public override void Update()
    {
        boss.rb.linearVelocity = dir * speed;

        float dist = Vector2.Distance(boss.transform.position, boss.player.position);

        // 충분히 가까워지면 다시 판단
        if (dist <= 6f)
        {
            boss.ChangeState(new TutoIdleBattleState(boss));
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
        boss.Attacking = true;                   // 
        timer = 0f;

        //  튜토보스 전용 패턴 가져오기
        currentPattern = boss.SelectBestPattern();

        // 또는 튜토보스 전용 패턴 배열에서 직접 꺼내기
        // currentPattern = boss.TutoPatterns[0];

        if (currentPattern != null)
        {
            currentPattern.Initialize(boss);     
            currentPattern.StartPattern();       
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
