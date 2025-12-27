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
        Vector3 targetPosition = summondrone.Resear_trans.position + (Vector3)summondrone.offset;
        summondrone.transform.position = targetPosition;
        if (summondrone.sightRange.IsPlayerInSight)
        {
            Debug.Log("플레이어 발견! Attack 상태로 전환.");
            summondrone.ChangeState(summondrone.SD_states[SummonDroneStateType.Attack]);
            return;
        }
        summondrone.R_directiontoDrone();
    }
    public override void Exit(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Idle State 종료");
    }
}

public class SD_Attackstate : SD_State
{
    float distance;
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Attack State 시작");
      
    }
    public override void Update(SummonDrone summondrone)
    {
        distance = Vector3.Distance(summondrone.transform.position, summondrone.Player_trans.position);
        if(distance <= summondrone.explosionRadius)
        {
            summondrone.Attack();
        }
        summondrone.Chase();
    }
    public override void Exit(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Attack State 종료");
    }
}

public class SD_Summonstate : SD_State
{
   float distance;
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Summon State 시작");
    }
    public override void Update(SummonDrone summondrone)
    {
        Vector3 target = summondrone.Resear_trans.position + (Vector3)summondrone.offset;

        // 드론이 타겟 위치를 향해 이동 
        if (target.x > summondrone.transform.position.x)
        {

            summondrone.transform.localScale = new Vector3(Mathf.Abs(summondrone.transform.localScale.x),summondrone.transform.localScale.y,summondrone.transform.localScale.z);
        }

        else if (target.x < summondrone.transform.position.x)
        {
            summondrone.transform.localScale = new Vector3(-Mathf.Abs(summondrone.transform.localScale.x),summondrone.transform.localScale.y,summondrone.transform.localScale.z);
        }

        summondrone.transform.position = Vector3.MoveTowards(summondrone.transform.position,target,summondrone.SD_Speed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(summondrone.transform.position, target);

        // 타겟 위치에 도착했는지 확인
        if (distanceToTarget < summondrone.arriveDistance) 
        {
            Debug.Log("소환 드론 목표 위치 도착. Idle 상태로 전환.");
 
            summondrone.Rigid.linearVelocity = Vector2.zero;
            
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
        summondrone.Rigid.linearVelocity = Vector2.zero;
    }
    public override void Update(SummonDrone summondrone)
    {
       
    }
    public override void Exit(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Dead State 종료");
    }
}