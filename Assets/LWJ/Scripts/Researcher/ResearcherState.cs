using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.RuleTile.TilingRuleOutput;


public abstract class ResearcherState 
{
    public abstract void Start(Researcher researcher);
    public abstract void Update(Researcher researcher);
    public abstract void Exit(Researcher researcher);
}

public class R_IdleState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
        Debug.Log("Researcher Idle State 시작");
    }
    public override void Update(Researcher researcher)
    {

        if (researcher.isDroneSummoned == false)
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
            {
                Debug.Log("플레이어 감지!");
                researcher.ChangeState(researcher.r_states[ResearcherStateType.Summon]);
                return;
            }     
        }
        else
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
            {         
                researcher.ChangeState(researcher.r_states[ResearcherStateType.Chase]);
                return;
            }
        }      
        researcher.Rigid.linearVelocity = Vector2.zero;
    }

    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Idle State 종료");
    }


}

public class R_WalkState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
        Debug.Log("Researcher Walk State 시작");
        researcher.animator.Play("R_Move");
    }
    public override void Update(Researcher researcher)
    {
        if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
        {
            if (!researcher.isDroneSummoned)
            {
                researcher.ChangeState(researcher.r_states[ResearcherStateType.Summon]);
                return;
            }

            if (researcher.aimRange != null && researcher.aimRange.IsPlayerInSight)
            {
                researcher.ChangeState(researcher.r_states[ResearcherStateType.Attack]);
                return;
            }

            researcher.ChangeState(researcher.r_states[ResearcherStateType.Chase]);
            return;
        }

        researcher.WallorLedgeFlip();

    }


    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher walk State 종료");

        researcher.Rigid.linearVelocity = Vector2.zero;
    }

}


public class R_SummonDroneState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
        Debug.Log("드론 소환");
        researcher.Rigid.linearVelocity = Vector2.zero;
        researcher.SummonDrone();
        researcher.animator.Play("R_Summon");
    }
    public override void Update(Researcher researcher)
    {

    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Summon Drone State 종료");
        
    }
}

public class  R_ChaseState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
        Debug.Log("추적");
        researcher.Armandbodyshotend();
        researcher.animator.Play("R_Move");
    }
    public override void Update(Researcher researcher)
    {

        if (researcher.CheckForObstacle() || researcher.CheckForLedge())
        {
            researcher.ChangeState(researcher.r_states[ResearcherStateType.Walk]);
            return;
        }
      

        if(researcher.aimRange != null && researcher.aimRange.IsPlayerInSight)
        {
            researcher.ChangeState(researcher.r_states[ResearcherStateType.Attack]);
            return;
        }
        researcher.Chase();
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("추적 상태 종료");      
    }
}
public class R_Attackstate : ResearcherState
{
    private bool active;

    public override void Start(Researcher researcher)
    {
        active = true;
        Debug.Log("연구원 공격!");
        if (!researcher.isbodylock)
        {
            researcher.FlipToTargetX(researcher.Player_Trans.position.x);
        }
        researcher.Armsetactive(active);
        researcher.animator.Play("R_attack");
        researcher.Rigid.linearVelocity = Vector2.zero;
    }
    public override void Update(Researcher researcher)
    {
        researcher.Attack();
    }
    public override void Exit(Researcher researcher)
    {
        active = false;
        Debug.Log("Researcher Attack State 종료");
        researcher.Armsetactive(active);
        researcher.Armandbodyshotend();
    }
 
}

public class R_Hitstate : ResearcherState
{
    private float hitDuration = 0.1f; 
    private float exitTime;
    public override void Start(Researcher researcher)
    {

        Debug.Log("Researcher Hit State 시작");
        exitTime = Time.time + hitDuration;
        researcher.Knockback();
    }
    public override void Update(Researcher researcher)
    {
        if (Time.time >= exitTime)
        {
            researcher.Rigid.linearVelocity = Vector2.zero;

            // 이전 상태가 공격/추적이면 거기로 복귀
            if (researcher.previousState is R_Attackstate || researcher.previousState is R_ChaseState)
            {
                researcher.ChangeState(researcher.previousState);
                return;
            }

            researcher.ChangeState(researcher.r_states[ResearcherStateType.Chase]);
        }
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Hit State 종료");
    }
}

public class R_Deadstate : ResearcherState
{

    public override void Start(Researcher researcher)
    {

        Debug.Log("Researcher Dead State 시작");
        researcher.Rigid.linearVelocity = Vector2.zero;
        researcher.animator.Play("R_Death");

        
    }
    public override void Update(Researcher researcher)
    {

    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Dead State 종료");
    }
}

