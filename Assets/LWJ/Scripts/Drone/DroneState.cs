using UnityEditor.Searcher;
using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class DroneState 
{
    public abstract void Start(Drone drone);
    public abstract void Update(Drone drone);
    public abstract void Exit(Drone drone);
}

public class D_Idlestate : DroneState
{
    float idleDuration;
    float waitTime = 1f;
    public override void Start(Drone drone) 
    {
        Debug.Log("Idle State 시작");
        idleDuration = 0f;
        drone.Rigid.linearVelocity = Vector2.zero;
        drone.animator.Play("D_idle");
    }
    public override void Update(Drone drone)
    {
        if (drone.sightrange.PlayerInSight != null)
        {
            drone.ChangeState(drone.State[DroneStateType.Chase]);
            return;
        }

        idleDuration += Time.deltaTime;
        if (idleDuration >= waitTime)
        {
            drone.ChangeState(drone.State[DroneStateType.Walk]);
        }
    }

    public override void Exit(Drone drone) { }


}

public class D_Walkstate : DroneState
{
    Vector3 nextPos;
    public override void Start(Drone drone)
    {
        Debug.Log("Walk State 시작");
        drone.animator.Play("D_Walk");
        nextPos = drone.PatrolPath.GetRandomPoint();
        float dir = nextPos.x - drone.transform.position.x;
        drone.FlipDrone(dir);
    }
    public override void Update(Drone drone)
    {
        if(drone.CheckForObstacle())
        {   
            drone.ChangeState(drone.State[DroneStateType.Idle]);
            return;
        }

        if(drone.sightrange.PlayerInSight != null)
        {
            drone.ChangeState(drone.State[DroneStateType.Chase]);
            return;
        }
        drone.Move(nextPos);
    }

    public override void Exit(Drone drone) { }
}


public class D_Attackstate : DroneState
{
    public override void Start(Drone drone) 
    {
        Debug.Log("Attack State 시작");
        drone.Rigid.linearVelocity = Vector2.zero;
        drone.Resetplayerposition();
        drone.isattack = true;
        drone.animator.Play("D_Attack");
    }
    public override void Update(Drone drone) 
    {
        drone.Attack();
    }
    public override void Exit(Drone drone)
    {

    }

}

public class D_Chasestate : DroneState
{
    public override void Start(Drone drone)
    {
        Debug.Log("드론 Chase State 시작");
        drone.animator.Play("D_Walk");
    }
    public override void Update(Drone drone)
    {
        if(drone.CheckForObstacle())
        {
            drone.Rigid.linearVelocity = Vector2.zero;
            drone.ChangeState(drone.State[DroneStateType.Return]);
            return;
        }

        if (drone.DistanceToPlayer() <= drone.Stat.moveDistance)
        {
            drone.ChangeState(drone.State[DroneStateType.Attack]);
            return;
        }
        drone.Chase();

    }
    public override void Exit(Drone drone) { }
}

public class D_Deadstate : DroneState
{
    LayerMask origin;
    public override void Start(Drone drone)
    {
        Debug.Log("Dead State 시작");
        drone.Rigid.linearVelocity = Vector2.zero;
        origin = drone.gameObject.layer;
        drone.gameObject.layer = LayerMask.NameToLayer("Invincible");
        drone.animator.Play("D_Dead");
        drone.waitgameobjectfalse();
        
    }
    public override void Update(Drone drone)
    {


    }
    public override void Exit(Drone drone) { drone.gameObject.layer = origin; }
}
public class D_Hitstate : DroneState
{
    private float hitDuration = 0.1f;
    private float exitTime;
    public override void Start(Drone drone)
    {
        Debug.Log("HIt State 시작");
        exitTime = hitDuration + Time.time;
        drone.Rigid.linearVelocity = Vector2.zero;
    }
    public override void Update(Drone drone)
    {
        if (Time.time >= exitTime)
        {
            if (drone.Pl_trans != null)
            {
                float direction = drone.Pl_trans.position.x - drone.transform.position.x;
                drone.FlipDrone(direction);
                drone.ChangeState(drone.State[DroneStateType.Chase]);
            }
            else
            {
                drone.ChangeState(drone.State[DroneStateType.Idle]);
            }
        }


    }
    public override void Exit(Drone drone) { }
}
public class D_EnhancedDroneState : DroneState
{
    LayerMask origin;
    public override void Start(Drone drone)
    {
        Debug.Log("Enhanced Drone State 시작");
        drone.Rigid.linearVelocity = Vector2.zero;
        origin = drone.gameObject.layer;
        drone.gameObject.layer = LayerMask.NameToLayer("Invincible");
        drone.animator.Play("D_Enhance");
    }
    public override void Update(Drone drone)
    {
        if (drone.Enhanced)
        {
             if (drone.DistanceToPlayer() <= drone.Stat.moveDistance)
             {
                 drone.ChangeState(drone.State[DroneStateType.Attack]);
             }
             else
             {
                 drone.ChangeState(drone.State[DroneStateType.Chase]);
             }   
        }
    }
    public override void Exit(Drone drone)
    {
        Debug.Log("Enhanced Drone State 종료");
        drone.gameObject.layer = origin;
    }
}
public class D_Returnstate : DroneState
{

    public override void Start(Drone drone)
    {
        Debug.Log("Return State 시작");
        drone.animator.Play("D_Walk");

    }
    public override void Update(Drone drone)
    {
        drone.ReturnToStartPoint();
        if (drone.sightrange.PlayerInSight)
        {
            drone.ChangeState(drone.State[DroneStateType.Chase]);
        }
      
    }
    public override void Exit(Drone drone)
    {
        drone.Rigid.linearVelocity = Vector2.zero;
    }
}

