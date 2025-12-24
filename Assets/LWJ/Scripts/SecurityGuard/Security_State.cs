using UnityEngine;

public abstract class Security_State 
{
    public abstract void Start(Security_Guard guard);
    public abstract void Update(Security_Guard guard);
    public abstract void Exit(Security_Guard guard);
}

public class Security_Idle : Security_State
{
    float idleDuration = 0f;
    float waitTime = 1f;
    public override void Start(Security_Guard guard)
    {
        Debug.Log("보안 요원 Idle State 시작");
        guard.animator.Play("Security_Idle");
    }
    public override void Update(Security_Guard guard)
    {
        idleDuration += Time.deltaTime;
        if (idleDuration >= waitTime)
        {
            Debug.Log("Idle State 종료, Walk State로 전환");
            float currentDir = Mathf.Sign(guard.transform.localScale.x);
            guard.FlipGuard(-currentDir);
            guard.ChangeState(guard.states[GuardStateType.Walk]);

            idleDuration = 0f;
        }
    }
    public override void Exit(Security_Guard guard)
    {
        // Idle 상태 종료 시의 행동 구현
    }

}

public class Security_Walk : Security_State
{
    public override void Start(Security_Guard guard)
    {
        Debug.Log("보안 요원 walk State 시작");
        guard.animator.Play("Security_Walk");
    }
    public override void Update(Security_Guard guard)
    {
        if(guard.CheckForObstacle() || !guard.CheckForLedge())
        {   
            guard.ChangeState(guard.states[GuardStateType.Idle]);
            return;
        }

        if(guard.sightRange.PlayerInSight)
        {
            guard.ChangeState(guard.states[GuardStateType.Chase]);
            return; 
        }

        guard.Move();
    }
    public override void Exit(Security_Guard guard)
    {
        // Idle 상태 종료 시의 행동 구현
    }

}

public class Security_Chase : Security_State
{
    public override void Start(Security_Guard guard)
    {
        guard.animator.Play("Security_Walk");
    }
    public override void Update(Security_Guard guard)
    {
        if(guard.CheckForObstacle() || guard.CheckForLedge())
        {
            guard.ChangeState(guard.states[GuardStateType.Idle]);
            return;
        }

       if(guard.DistanceToPlayer() <= guard.Stat.moveDistance)
       {
            guard.ChangeState(guard.states[GuardStateType.Attack]);
            return;
       }
       else
       {
           guard.Chase();
       }
    }
    public override void Exit(Security_Guard guard)
    {
        // Idle 상태 종료 시의 행동 구현
    }

}

public class Security_Attack : Security_State
{
    public override void Start(Security_Guard guard)
    {
        guard.animator.Play("Security_Attack");
    }
    public override void Update(Security_Guard guard)
    {
       
    }
    public override void Exit(Security_Guard guard)
    {
        // Idle 상태 종료 시의 행동 구현
    }

}

public class Security_Death : Security_State
{
    public override void Start(Security_Guard guard)
    {
        guard.animator.Play("Security_Death");
    }
    public override void Update(Security_Guard guard)
    {
        // Idle 상태에서의 행동 구현
    }
    public override void Exit(Security_Guard guard)
    {
        // Idle 상태 종료 시의 행동 구현
    }

}

public class Security_Hit : Security_State
{
    public override void Start(Security_Guard guard)
    {
        guard.animator.Play("Security_Idle");
    }
    public override void Update(Security_Guard guard)
    {
        // Idle 상태에서의 행동 구현
    }
    public override void Exit(Security_Guard guard)
    {
        // Idle 상태 종료 시의 행동 구현
    }

}