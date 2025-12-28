using UnityEngine;
using static UnityEngine.UI.Image;

public  abstract class SD_State 
{
    public abstract void Start(SummonDrone summondrone);
    public abstract void Update(SummonDrone summondrone);
    public abstract void Exit(SummonDrone summondrone);
}

public class SD_Idlestate : SD_State
{
    float idleDuration;
    float waitTime = 1f;
    public override void Start(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Idle State 시작");
        summondrone.Rigid.linearVelocity = Vector2.zero;
        summondrone.animator.Play("Idle");
    }
    public override void Update(SummonDrone summondrone)
    {
        idleDuration += Time.deltaTime;
        if (idleDuration >= waitTime)
        {
            summondrone.ChangeState(summondrone.SD_states[SummonDroneStateType.Chase]);
        }
    }
    public override void Exit(SummonDrone summondrone) 
    {
        Debug.Log("SummonDrone Idle State 종료");
    }
}

public class SD_ChaseState : SD_State
{
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Chase State 시작");
        summondrone.animator.Play("Idle");
    }
    public override void Update(SummonDrone summondrone)
    {
        float distance = Vector2.Distance(summondrone.transform.position, summondrone.Player_trans.position);

        if (distance <= summondrone.explosionRadius)
        {
            summondrone.ChangeState(summondrone.SD_states[SummonDroneStateType.Attack]);
            return;
        }
        
        summondrone.Chase();
    }
    public override void Exit(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Chase State 종료");

    }
}

public class SD_Attackstate : SD_State
{

    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Attack State 시작");      
    }
    public override void Update(SummonDrone summondrone)
    {
         summondrone.Attack();
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

        // 드론이 타겟 위치를 향해 이동 
        if (target.x > summondrone.transform.position.x)
        {
            summondrone.transform.localScale = new Vector3(Mathf.Abs(summondrone.transform.localScale.x),summondrone.transform.localScale.y,summondrone.transform.localScale.z);
        }

        else if (target.x < summondrone.transform.position.x)
        {
            summondrone.transform.localScale = new Vector3(-Mathf.Abs(summondrone.transform.localScale.x),summondrone.transform.localScale.y,summondrone.transform.localScale.z);
        }

        Vector2 direction = (target - summondrone.transform.position);

        summondrone.Rigid.linearVelocity = direction.normalized * summondrone.SD_Speed;
        

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
    LayerMask origin;
    public override void Start(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Dead State 시작");
        origin = summondrone.gameObject.layer;
        summondrone.gameObject.layer = LayerMask.NameToLayer("Invincible");
        summondrone.Rigid.linearVelocity = Vector2.zero;
        summondrone.animator.Play("Dead");  
    }
    public override void Update(SummonDrone summondrone)
    {
       
    }
    public override void Exit(SummonDrone summondrone)
    {
        Debug.Log("SummonDrone Dead State 종료");
        summondrone.gameObject.layer = origin;
    }
}