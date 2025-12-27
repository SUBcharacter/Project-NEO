using UnityEditor.Searcher;
using UnityEngine;

public abstract class DroneState 
{
    public abstract void Start(Drone drone);
    public abstract void Update(Drone drone);
    public abstract void Exit(Drone drone);
}

public class D_Idlestate : DroneState
{
    float idleDuration = 0f;
    float waitTime = 1f;
    public override void Start(Drone drone) 
    {
        Debug.Log("Idle State 시작");
        drone.Rigid.linearVelocity = Vector2.zero;
        drone.animator.Play("D_idle");
    }
    public override void Update(Drone drone)
    {
        idleDuration += Time.deltaTime;
        if (idleDuration >= waitTime)
        {
            Debug.Log("Idle State 종료, Walk State로 전환");
            float currentDir = Mathf.Sign(drone.transform.localScale.x);
            drone.FlipDrone(-currentDir);
            drone.ChangeState(drone.State[DroneStateType.Walk]);

            idleDuration = 0f;
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

        if(drone.sightrange.PlayerInSight)
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
            drone.ChangeState(drone.State[DroneStateType.Return]);
            return;
        }

        if(drone.aimrange.PlayerInSight)
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
    public override void Start(Drone drone)
    {
        Debug.Log("Dead State 시작");
        drone.Rigid.linearVelocity = Vector2.zero;
        drone.animator.Play("D_Dead");
        drone.waitgameobjectfalse();
        
    }
    public override void Update(Drone drone)
    {


    }
    public override void Exit(Drone drone) { }
}
public class D_Hitstate : DroneState
{
    private float hitDuration = 0.1f;
    private float exitTime;
    public override void Start(Drone drone)
    {
        Debug.Log("HIt State 시작");
        exitTime = hitDuration + Time.time;

    }
    public override void Update(Drone drone)
    {
        if (Time.time >= exitTime)
        {
            drone.Rigid.linearVelocity = Vector2.zero;

            if (drone.sightrange.PlayerInSight && drone.aimrange.PlayerInSight)
            {
                drone.ChangeState(drone.State[DroneStateType.Attack]);
            }
            else
            {
                drone.ChangeState(drone.State[DroneStateType.Chase]);
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
             if (drone.aimrange.PlayerInSight)
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
        drone.animator.Play("D_Walk");
       
    }
    public override void Update(Drone drone)
    {
        drone.ReturnToStartPoint();
        if (!drone.CheckForObstacle())
        {
            if (drone.sightrange.PlayerInSight)
            {
                drone.ChangeState(drone.State[DroneStateType.Chase]);
            }
        }
    }
    public override void Exit(Drone drone)
    {
        drone.Rigid.linearVelocity = Vector2.zero;
    }
}

