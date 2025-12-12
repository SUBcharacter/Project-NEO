using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BossState
{
    protected BossAI boss;
    protected float bossRadius = 4f;
    protected float bossSpeed = 3f;
    public BossState(BossAI boss) => this.boss = boss;
    public abstract void Start();
    public abstract void Update();
    public abstract void Exit();
    public virtual void OnAnimationEvent(string eventName) { }
}

public class BossIdleState : BossState
{
    bool isPatrolling = false;
    float distanceToPlayer = 99f;
    public BossIdleState(BossAI boss) : base(boss) { }

    public override async void Start()
    {
        isPatrolling = true;

        if (boss.player.transform != null && distanceToPlayer < 8f)
        {
            isPatrolling = false;
            boss.ChangeState(new AttackingState(boss));
            return;
        }
        await PatrolRoutine();
    }
    private async Task PatrolRoutine()
    {
        while (isPatrolling)
        {
            float timer = 0;
            boss.FaceTarget(boss.transform.position + Vector3.left);
            while (timer < 1f && isPatrolling)         // 좌로 패트롤
            {
                timer += Time.deltaTime;
                boss.transform.position += bossSpeed * Time.deltaTime * Vector3.left;
                await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
            }
            if (!isPatrolling) break;
            await Awaitable.WaitForSecondsAsync(2f, boss.DestroyCancellationToken);


            boss.FaceTarget(boss.transform.position + Vector3.right);
            timer = 0;
            while (timer < 1f && isPatrolling)         // 우로 패트롤
            {
                timer += Time.deltaTime;
                boss.transform.position += bossSpeed * Time.deltaTime * Vector3.right;
                await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
            }
            if (!isPatrolling) break;
            await Awaitable.WaitForSecondsAsync(2f, boss.DestroyCancellationToken);
        }
    }

    public override void Update()
    {
        distanceToPlayer = Vector2.Distance(boss.transform.position, boss.player.position);
        //Debug.Log(distanceToPlayer);
        if (boss.player.transform != null && distanceToPlayer < 13f)
        {
            isPatrolling = false;
            boss.ChangeState(new AttackingState(boss));
            return;
        }

    }
    public override void Exit()
    {
        isPatrolling = false;
    }

}

public class BossCoolDownState : BossState
{
    private float cooldownTime;
    public BossCoolDownState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        switch(boss.CurrentPhase.phaseName)
        {
            case "Phase 1":
                cooldownTime = 2.0f;
                break;
            case "Phase 2":
                cooldownTime = 1.5f;
                break;
            case "Berserk":
                cooldownTime = 1.5f;
                break;
            default:          
                break;
        }        
    }
    public override void Update()
    {
        if (cooldownTime > 0)
        {
            cooldownTime -= Time.deltaTime;
        }
        else
        {
            boss.ChangeState(new AttackingState(boss));
        }
    }
    public override void Exit()
    {
        // 흠..
        switch (boss.CurrentPhase.phaseName)
        {
            case "Phase 1":
                cooldownTime = 2.0f;
                break;
            case "Phase 2":
                cooldownTime = 1.5f;
                break;
            case "Berserk":
                cooldownTime = 1.0f;
                break;
            default:
                break;
        }
    }
}


// 넣을까 말까
//public class ChasingState : BossState
//{
//    public override void Start()
//    {  
//
//    }
//    public override void Update()
//    {
//     
//    }
//    public override void Exit()
//    {
//       
//    }
//}

public class AttackingState : BossState
{
    private BossPattern currentPattern;
    private int attackCounter;            // 이걸 쓸지 안 쓸지 고민 좀 해야함. 쓴다면 -> 패턴 2개 정도 하고 chasing 상태로 넘어가는 식으로
    public AttackingState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        switch (boss.CurrentPhase.phaseName)
        {
            case "Phase 1":
                attackCounter = 1;
                break;
            case "Phase 2":
                attackCounter = 2;
                break;
            case "Berserk":
                attackCounter = 4;
                break;
            default:                
                break;
        }     
        ExecutePattern();
    }

    private void ExecutePattern()
    {
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);

        List<BossPattern> candidatePatterns = null;

        // 사정거리별 
        if (distance < 7.0f) candidatePatterns = boss.CurrentPhase.shortPattern;
        else if (distance < 12.0f) candidatePatterns = boss.CurrentPhase.middlePattern;
        else candidatePatterns = boss.CurrentPhase.longPattern;

        if (candidatePatterns == null || candidatePatterns.Count == 0) // 혹시 모를 예외처리
        {
            //candidatePatterns = boss.CurrentPhase.shortPattern;
            candidatePatterns = boss.CurrentPhase.middlePattern;
            //candidatePatterns = boss.CurrentPhase.longPattern;
        }

        int index = Random.Range(0, candidatePatterns.Count);
        currentPattern = candidatePatterns[index];

        currentPattern.Initialize(boss);
        currentPattern.StartPattern();
    }
    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "AttackEnd")
        {
            currentPattern.ExitPattern();

            attackCounter--;
            if (attackCounter > 0)
            {
                ExecutePattern();
            }
            else
            {
                boss.ChangeState(new BossCoolDownState(boss));
            }
        }
        else
        {
            currentPattern?.OnAnimationEvent(eventName);
        }
    }
    public override void Update() => currentPattern?.UpdatePattern();
    public override void Exit() => currentPattern?.ExitPattern();
}
