using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;

public class TutoBossState : BossState
{
    protected TutoBossAI tutoAI;
    protected float tutoBossRadius = 4f;
    protected float tutoBossSpeed = 3f;
    public TutoBossState(BossAI boss) : base(boss)
    {
        tutoAI = boss as TutoBossAI;
    }

    public override void Exit()
    {

    }

    public override void Start()
    {

    }

    public override void Update()
    {

    }


}

public class TutoIdleState : TutoBossState
{
    private bool isPatrolling = false;
    private float detectRange = 8f;

    public TutoIdleState(BossAI boss) : base(boss) { }

    public override async void Start()
    {
        isPatrolling = true;

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

            //float timer = 0;
            //boss.FaceTarget(boss.transform.position + Vector3.left);

            //while (timer < 1f && isPatrolling)
            //{
            //    timer += Time.deltaTime;
            //    boss.transform.position += tutoBossSpeed * Time.deltaTime * Vector3.left;
            //    await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
            //}

            //if (!isPatrolling) break;
            //await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);

            //// 오른쪽
            //timer = 0;
            //boss.FaceTarget(boss.transform.position + Vector3.right);

            //while (timer < 1f && isPatrolling)
            //{
            //    timer += Time.deltaTime;
            //    boss.transform.position += tutoBossSpeed * Time.deltaTime * Vector3.right;
            //    await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
            //}

            //if (!isPatrolling) break;
            //await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);
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

        // Chase 전환 조건
        if (dist < detectRange)
        {
            isPatrolling = false;   // 패트롤 즉시 종료
            boss.ChangeState(new TutoChaseState(boss));
        }
    }

    public override void Exit()
    {
        // 상태 변경시 반드시 패트롤 종료
        isPatrolling = false;
    }
}


public class TutoChaseState : TutoBossState
{
    private float chaseSpeed = 2.5f;


    public TutoChaseState(BossAI boss) : base(boss) { }


    public override void Start()
    {
        boss.animator.SetTrigger("Walk");
    }

    public override void Update()
    {
        if (boss.player == null) return;

        float targetX = boss.player.position.x;
        float bossY = boss.transform.position.y;

        Vector2 targetPos = new Vector3(targetX, bossY, boss.transform.position.z);

        boss.FaceTarget(boss.player.position);

        boss.transform.position = Vector3.MoveTowards
            (boss.transform.position, targetPos, chaseSpeed * Time.deltaTime);

        float dist = Mathf.Abs(boss.player.position.x - boss.transform.position.x);

        if (dist > 6f)
        {
            boss.ChangeState(new TutoAttackingState(boss));
        }
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

        if (currentPattern == null)
        {
            boss.ChangeState(new TutoCoolDownState(boss, 1f));
            return;
        }

        currentPattern.Initialize(boss);
        currentPattern.StartPattern();
    }

    private BossPattern SelectPattern()
    {
        // 1) 튜토 순서대로
        if (tuto.tutorialIndex < tuto.tutorialSequence.Count)
        {
            return tuto.tutorialSequence[tuto.tutorialIndex++];
        }

        // 2) 이후 랜덤
        if (tuto.randomPatterns.Count > 0)
        {
            int idx = Random.Range(0, tuto.randomPatterns.Count);
            return tuto.randomPatterns[idx];
        }

        return null;
    }

    public override void Update()
    {
        currentPattern?.UpdatePattern();
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "AttackEnd")
        {
            currentPattern?.ExitPattern();
            boss.ChangeState(new TutoCoolDownState(boss, 1.5f));
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

        if (timer <= 0)
        {
            boss.ChangeState(new TutoChaseState(boss));
        }
    }

    public override void Exit() { }
}