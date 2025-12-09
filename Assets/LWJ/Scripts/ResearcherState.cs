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
        researcher.statetime = Time.time + researcher.Idlewaittime;

    }
    public override void Update(Researcher researcher)
    {

        if (researcher.isDroneSummoned == false)
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
            {
                Debug.Log("플레이어 감지!");
                researcher.ChangeState(researcher.R_States[2]);
                return;
            }     
        }
        else
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
            {         
                researcher.ChangeState(researcher.R_States[3]);
                return;
            }
        }

       
        researcher.rb.linearVelocity = Vector2.zero;

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
        researcher.PlayWalk();
    }
    public override void Update(Researcher researcher)
    {

        if (researcher.isDroneSummoned == false)
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight )
            {
                Debug.Log("플레이어 감지!");
                researcher.ChangeState(researcher.R_States[2]);
                return;
            }
        }
        else
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
            {
                researcher.ChangeState(researcher.R_States[3]);
                return;
            }
        }

        if (researcher.CheckForObstacle(researcher) || researcher.CheckForLedge(researcher))
        {
          
          
            researcher.Movedistance *= -1;
            researcher.FlipResearcher(researcher, researcher.Movedistance);
            researcher.ChangeState(researcher.R_States[0]);
            return;
        }
        else
        {
            researcher.PatrolMove();
        }
      
    }


    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher walk State 종료");
        researcher.StopWalk();
        researcher.rb.linearVelocity = Vector2.zero;
    }

}

public class R_SummonDroneState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
       Debug.Log("드론 소환");
        researcher.PlaySummon();

        researcher.rb.linearVelocity = Vector2.zero;
        researcher.statetime = Time.time + researcher.WaitTimer;
        researcher.SummonDrone();

    }
    public override void Update(Researcher researcher)
    {
    
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Summon Drone State 종료");
        researcher.StopSummon();
    }
}   

public class R_Attackstate : ResearcherState
{
    private float fireRate = 1f; 
    private float nextFireTime;
    private float directionToPlayer;
    private bool active;
    public override void Start(Researcher researcher)
    {
        active = true;
        Debug.Log("연구원 공격!");
        nextFireTime = fireRate;
        researcher.Armsetactive(active);  
        researcher.PlayAttack();    
    }
    public override void Update(Researcher researcher)
    {

        if (researcher.CheckForObstacle(researcher) || researcher.CheckForLedge(researcher))
        {

            researcher.rb.linearVelocity = new Vector2(0, researcher.rb.linearVelocity.y);
            researcher.StopAttack();
            researcher.ChangeState(researcher.R_States[0]); 
            return;
        }

        researcher.MovetoPlayer();
        researcher.Aimatplayer();
        if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
        { 
            nextFireTime -= Time.deltaTime;
            if (0 >= nextFireTime)
            {
                researcher.ShootBullet();
                nextFireTime = fireRate; 
            }
        }

    }
    public override void Exit(Researcher researcher)
    {
        active = false;
        Debug.Log("Researcher Attack State 종료");
        researcher.Armsetactive(active);
        researcher.StopAttack();
    }

 
}

public class R_Hitstate : ResearcherState
{
    private float hitDuration = 0.1f; // 넉백 지속 시간
    private float exitTime;
    float directionToPlayer;
    public override void Start(Researcher researcher)
    {

        Debug.Log("Researcher Hit State 시작");
        exitTime = Time.time + hitDuration;
        directionToPlayer = researcher.Player_Trans.position.x - researcher.transform.position.x;
        researcher.Knockback();
    }
    public override void Update(Researcher researcher)
    {
        if (Time.time >= exitTime)
        {
         
            researcher.FlipResearcher(researcher, directionToPlayer);
            researcher.rb.linearVelocity = Vector2.zero;
            researcher.ChangeState(researcher.R_States[3]);
        }
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Hit State 종료");
    }
}

