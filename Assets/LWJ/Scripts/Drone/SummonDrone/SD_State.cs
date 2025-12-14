using UnityEngine;

public  abstract class SD_State 
{
    public abstract void Start(SummonDrone summondrone);
    public abstract void Update(SummonDrone summondrone);
    public abstract void Exit(SummonDrone summondrone);
}

public class SD_Idlestate : SD_State
{
    public override void Start(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Idle State 시작");
        summondrone.animator.Play("Idle");
    }
    public override void Update(SummonDrone summondrone)
    {
        if(summondrone.sightRange.IsPlayerInSight)
        {
            Debug.Log("플레이어 발견! Attack 상태로 전환.");
            summondrone.ChangeState(summondrone.SD_states[SummonDroneStateType.Attack]);
            return;
        }
    }
    public override void Exit(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Idle State 종료");
    }
}

public class SD_Attackstate : SD_State
{
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Attack State 시작");
        summondrone.StartExplosionTimer();
    }
    public override void Update(SummonDrone summondrone)
    {
        if(summondrone.sightRange.IsPlayerInSight)
        {
            summondrone.transform.position = Vector3.MoveTowards(summondrone.transform.position, summondrone.Player_trans.position, summondrone.SD_Speed * Time.deltaTime);
            float directionX = summondrone.Player_trans.position.x - summondrone.transform.position.x;
        }
    }
    public override void Exit(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Attack State 종료");
    }
}

public class SD_Summonstate : SD_State
{
   
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Summon State 시작");
    }
    public override void Update(SummonDrone summondrone)
    {
        Vector3 target = summondrone.Resear_trans.position + (Vector3)summondrone.offset;


        summondrone.transform.position = Vector3.MoveTowards( summondrone.transform.position,target,summondrone.SD_Speed * Time.deltaTime);


        float distanceToTarget = Vector3.Distance(summondrone.transform.position, target);

        if (distanceToTarget < summondrone.Arriveposition) 
        {
            Debug.Log("소환 드론 목표 위치 도착. Idle 상태로 전환.");

            if (summondrone.Rigid != null)
            {
                summondrone.Rigid.linearVelocity = Vector2.zero;
            }

            summondrone.ChangeState(summondrone.SD_states[SummonDroneStateType.Idle]);
        }

    }
    public override void Exit(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Summon State 종료");
    }
}

public class SD_DeadState : SD_State
{
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Dead State 시작");
        summondrone.animator.Play("Dead");  
    }
    public override void Update(SummonDrone summondrone)
    {
        // 사망 로직 구현
    }
    public override void Exit(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Dead State 종료");
    }
}