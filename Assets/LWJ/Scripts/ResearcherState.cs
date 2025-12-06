using UnityEngine;
using UnityEngine.EventSystems;

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
        Vector3 movement = Vector3.right * researcher.Movedistance * researcher.R_Speed * Time.deltaTime;
        researcher.transform.position += movement;
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
    public override void Start(Researcher researcher)
    {
        Debug.Log("연구원 공격!");
        nextFireTime = fireRate;
    }
    public override void Update(Researcher researcher)
    {
        if(researcher.sightRange != null && !researcher.sightRange.IsPlayerInSight)
        {
            Debug.Log("플레이어 시야에서 벗어남");
            researcher.ChangeState(researcher.R_States[0]);
            return;
        }
        else
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
    public override void Start(Researcher researcher)
    {
        Debug.Log("Researcher Hit State 시작");
    }
    public override void Update(Researcher researcher)
    {
        // 피격 상태에서의 동작 구현
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Hit State 종료");
    }
}

