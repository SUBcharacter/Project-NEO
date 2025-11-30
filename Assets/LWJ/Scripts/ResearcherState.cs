using UnityEngine;

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
        Debug.Log("Idle State 시작");
    }
    public override void Update(Researcher researcher)
    {
        if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight && !researcher.isDroneSummoned)
        {
            Debug.Log("플레이어 감지!");
            researcher.ChangeState(researcher.R_States[1]);
            
            return;
        }
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Idle State 종료");
    }
}   

public class R_SummonDroneState : ResearcherState
{
    public override void Start(Researcher researcher)
    {
        int rand = Random.Range(0, researcher.dronespawnpoints.Length);
        Transform spawnPos = researcher.dronespawnpoints[rand];

        Drone newDrone = GameObject.Instantiate(researcher.D_prefab, spawnPos.position, Quaternion.identity);
        researcher.isDroneSummoned = true;
        newDrone.Init(researcher.transform, researcher.Player_Trans);
        Debug.Log("드론 소환");

        researcher.ChangeState(researcher.R_States[0]);


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
    public override void Start(Researcher researcher)
    {

    }
    public override void Update(Researcher researcher)
    {

    }
    public override void Exit(Researcher researcher)
    {
    }
}

