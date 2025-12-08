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
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight && !researcher.isDroneSummoned)
            {
                Debug.Log("플레이어 감지!");
                researcher.ChangeState(researcher.R_States[1]);
                return;
            }     
        }
        else
        {
            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
            {         
                researcher.ChangeState(researcher.R_States[2]);
                return;
            }
        }

        if (researcher.CheckForObstacle(researcher) || researcher.CheckForLedge(researcher))
        {
            researcher.Movedistance *= -1;
            researcher.FlipResearcher(researcher, researcher.Movedistance); 
        }
       researcher.PatrolMove();
    }


    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Idle State 종료");
    }

}   

public class R_SummonDroneState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
       Debug.Log("드론 소환");

      
        researcher.rb.linearVelocity = Vector2.zero;
        researcher.statetime = Time.time + researcher.WaitTimer;
        researcher.SummonDrone();

    }
    public override void Update(Researcher researcher)
    {
        researcher.StopResearcherTimer();
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Summon Drone State 종료");
    }
}   

public class R_Attackstate : ResearcherState
{
    private float fireRate = 1f; 
    private float nextFireTime;
    private float directionToPlayer;
    public override void Start(Researcher researcher)
    {
        Debug.Log("연구원 공격!");
        nextFireTime = fireRate;
    }
    public override void Update(Researcher researcher)
    {

        if (researcher.CheckForObstacle(researcher) || researcher.CheckForLedge(researcher))
        {
            researcher.rb.linearVelocity = new Vector2(0, researcher.rb.linearVelocity.y);
            researcher.ChangeState(researcher.R_States[0]); 
            return;
        }

        researcher.MovetoPlayer();
        if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
        { 
            nextFireTime -= Time.deltaTime;
            if (0 >= nextFireTime)
            {
                Vector2 shootDirection = researcher.GetcurrentVect2();
                researcher.ShootBullet(shootDirection);
                nextFireTime = fireRate; 
            }
        }

    }
    public override void Exit(Researcher researcher)
    {
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
            researcher.ChangeState(researcher.R_States[2]);
        }
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Hit State 종료");
    }
}

