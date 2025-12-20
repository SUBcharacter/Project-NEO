using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;

public class TutoIdleState : BossState
{
    // SO로 뺄 예정
    private float attackRange = 1.5f;
    private float retreatRange = 1f;  // 너무 가까움 -> 스웨이
    private float dashRange = 6f;     // 멀다 -> 대시
    public TutoIdleState(BossAI boss) : base(boss)
    {

    }

    public override void Start()
    {
        float dist = Mathf.Abs(boss.player.position.x - boss.transform.position.x);

        // 1) 너무 가까우면 → 뒤로 빠지는 스웨이
        if (dist < retreatRange)
        {
            boss.ChangeState(new TutoSwayState(boss));
            return;
        }

        // 2) 너무 멀면 → 붙기 (더킹)
        if (dist > dashRange)
        {
            boss.ChangeState(new TutoDashState(boss));
            return;
        }

        // 3) 적정거리 → 공격 전환
        if (dist <= attackRange)
        {
            boss.ChangeState(new TutoAttackingState(boss));
            return;
        }
        boss.animator.SetTrigger("Idle");
        return;

    }
    public override void Update() { }

    public override void Exit() { }


}

public class TutoSwayState : BossState
{
    private Vector2 dir;
    private float speed = 2f;
    private float returnSpeed = 0.5f;

    public TutoSwayState(BossAI boss) : base(boss)
    {
    }
    public override void Start()
    {
        boss.animator.SetTrigger("Sway");
        boss.FaceTarget(boss.player.position);

        // 플레이어가 오른쪽 → 나는 왼쪽으로 빠져야 함
        float playerDir = Mathf.Sign(boss.player.position.x - boss.transform.position.x);

        dir = new Vector2(-playerDir, 0); // 반대 방향
    }

    public override void Update()
    {
        // 감속하면서 뒤로 빠지기
        speed = Mathf.Lerp(speed, 0, 5f * Time.deltaTime);
        boss.rb.linearVelocity = new Vector2(dir.x * speed, 0);

        // 충분히 감속되면 Idle로 복귀
        if (speed <= returnSpeed)
        {
            boss.ChangeState(new TutoIdleState(boss));
        }
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }


}

public class TutoDashState : BossState
{
    private Vector2 dir;
    private float dashSpeed = 12f;   // 순간 가속 속도 (SO)
    private float dashTime = 0.15f;  // 유지 시간 (SO)
    private float timer = 0f;
    public TutoDashState(BossAI boss) : base(boss)
    {
    }
    public override void Start()
    {
        boss.animator.SetTrigger("Dash");
        boss.FaceTarget(boss.player.position);
        dir = (boss.player.position - boss.transform.position).normalized;
    }
    public override void Update()
    {
        timer += Time.deltaTime;

        // 순간 가속
        boss.rb.linearVelocity = new Vector2(dir.x * dashSpeed, 0);

        if (timer >= dashTime)
        {
            // dash 이후 다시 Idle로
            boss.ChangeState(new TutoIdleState(boss));
        }
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }


}



/// <summary>
/// 어태킹이랑 쿨다운은 모르겠음 일단 가져옴
/// </summary>
public class TutoAttackingState : AttackingState
{
    public TutoAttackingState(BossAI boss, BossPattern pattern = null) : base(boss, pattern)
    {
    }
}

public class TutoCoolDownState : BossCoolDownState
{
    public TutoCoolDownState(BossAI boss) : base(boss)
    {
    }
}
