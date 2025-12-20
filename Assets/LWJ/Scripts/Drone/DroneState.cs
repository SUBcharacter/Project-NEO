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
    public override void Start(Drone drone)
    {
        Debug.Log("Walk State 시작");
        drone.animator.Play("D_Walk");
 
    }
    public override void Update(Drone drone)
    {
        if(drone.CheckForObstacle())
        {
            drone.ChangeState(drone.State[DroneStateType.Idle]);
        }

        if(drone.sightrange.PlayerInSight)
        {
            drone.ChangeState(drone.State[DroneStateType.Chase]);
        }
        drone.Move();
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
    public override void Start(Drone drone)
    {
        Debug.Log("Enhanced Drone State 시작");
    }
    public override void Update(Drone drone)
    {
        // 여기에 강화된 드론의 행동 로직을 추가하세요.
    }
    public override void Exit(Drone drone)
    {
        Debug.Log("Enhanced Drone State 종료");
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

