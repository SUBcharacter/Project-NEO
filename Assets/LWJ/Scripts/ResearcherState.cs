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
    float R_Speed = 2f;
    float Movedistance = 1f;

    private float wallCheckDistance = 0.5f; 
    private float groundCheckDistance = 0.8f;
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

        if (CheckForObstacle(researcher) || CheckForLedge(researcher))
        {
            Movedistance *= -1;
            researcher.FlipResearcher(researcher, Movedistance); 
        }

        Vector3 movement = Vector3.right * Movedistance * R_Speed * Time.deltaTime;
        researcher.transform.position += movement;

    }

    public bool CheckForObstacle(Researcher researcher)
    {
        Vector2 checkDirection = (Movedistance > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(researcher.transform.position,checkDirection,wallCheckDistance,researcher.wallLayer);

        return hit.collider != null;
    }

    private bool CheckForLedge(Researcher researcher)
    {
 
        Vector3 footPosition = researcher.transform.position;
        footPosition.x += Movedistance * 0.3f;
  
        RaycastHit2D hit = Physics2D.Raycast(footPosition,Vector2.down,groundCheckDistance,researcher.groundLayer);

        return hit.collider == null;
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
        int rand = Random.Range(0, researcher.dronespawnpoints.Length);
        Transform spawnPos = researcher.dronespawnpoints[rand];

        GameObject newDrone = GameObject.Instantiate(researcher.D_prefab, spawnPos.position, Quaternion.identity);
        researcher.isDroneSummoned = true;
        Drone drone = newDrone.GetComponent<Drone>();
        drone.SummonInit(researcher.transform, researcher.Player_Trans);
        
        Debug.Log("드론 소환");
        researcher.WaitDronetimer();

    }
    public override void Update(Researcher researcher)
    {
       
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

