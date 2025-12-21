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

    // 미니님 기준 밑의 코드들은 사실상 필요가 없음   boss.ChangeState(new AttackingState(boss)); 를 제외하곤 필요가 없음 
    // 그럼 어케해야하는가?
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

        // 튜토보스 페이즈 관련 코드 (예시 코드 이런 방향으로 갈 수도 있다는 것)
        //if (boss.AllPhase[0].phaseName == "TutoPhase")
        //{
        //    boss.ChangeState(new AttackingState(boss));
        //    return;
        //}
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
    private float timer;
    public BossCoolDownState(BossAI boss) : base(boss) { }

    public override void Start()
    {
        // Phase에 설정된 휴식 시간 가져오기
        timer = boss.CurrentPhase.restTime;

        // (선택) 여기서 플레이어와의 거리를 체크해서 너무 멀면 'ChasingState'로 갈 수도 있음      
    }
    public override void Update()
    {
        timer -= Time.deltaTime;
                
        boss.FaceTarget(boss.player.position);

        if (timer <= 0)
        {        
            boss.ChangeState(new AttackingState(boss));
        }
    }
    public override void Exit() { }
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

    public AttackingState(BossAI boss, BossPattern pattern = null) : base(boss) 
    {
        this.currentPattern = pattern;
    }

    public override void Start()
    {
        if (currentPattern == null)
        {
            currentPattern = boss.SelectBestPattern();
        }

        // 그래도 없으면 (전부 쿨타임 or 거리 안맞음) -> 짧은 대기(CoolDown) 혹은 추격(Move)
        if (currentPattern == null)
        {
            boss.ChangeState(new BossCoolDownState(boss));
            return;
        }

        currentPattern.Initialize(boss);
        currentPattern.StartPattern();
    }
    
    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "AttackEnd") 
        {
            currentPattern.ExitPattern();
            TryComboOrExit();
        }
        else
        {
            currentPattern?.OnAnimationEvent(eventName);
        }
    }
    private void TryComboOrExit()
    {
        // 콤보 체크
        if (currentPattern.comboPatterns != null && currentPattern.comboPatterns.Count > 0)
        {
            float roll = Random.Range(0f, 1f);
            if (roll <= currentPattern.comboChance) // 확률 성공?
            {
                // 다음 패턴 랜덤 선택
                int idx = Random.Range(0, currentPattern.comboPatterns.Count);
                BossPattern nextPattern = currentPattern.comboPatterns[idx];

                // 쿨타임 무시하고 바로 다음 공격 상태로 전이 (콤보)
                boss.ChangeState(new AttackingState(boss, nextPattern));
                return;
            }
        }
        // 콤보 없으면
        boss.ChangeState(new BossCoolDownState(boss));
    }
    public override void Update() => currentPattern?.UpdatePattern();
    public override void Exit() => currentPattern?.ExitPattern();
}


public class GroggyState : BossState
{
    private float groggyTimer;
    public GroggyState(BossAI boss,float time) : base(boss)
    {
        groggyTimer = time;
    }

    public override void Start()
    {
        Debug.Log("Groggy상태임");
        boss.animator.SetTrigger("Groggy");
    }
    public override void Update()
    {
        groggyTimer -= Time.deltaTime;

        if (groggyTimer < 0f)
        {
            // 시간 다 되면 ChangeState
            boss.ChangeState(new AttackingState(boss));
        }
    }
    public override void Exit() 
    {
        boss.RecoverPoise();
    }
}