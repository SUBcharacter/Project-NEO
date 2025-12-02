using UnityEngine;

public abstract class DroneState 
{
    public abstract void Start(Drone drone);
    public abstract void Update(Drone drone);
    public abstract void Exit(Drone drone);
}

public class D_Idlestate : DroneState
{
    public override void Start(Drone drone) 
    {

    }
    public override void Update(Drone drone)
    {
        Vector3 target = drone.Resear_trans.position + (Vector3)drone.offset; 
        drone.transform.position = Vector3.MoveTowards(drone.transform.position, target, drone.D_speed * Time.deltaTime);
  
        if (Vector3.Distance(drone.transform.position, target) < 0.1f)
        {
            drone.WaitDroneandattackstate();
           
        }
    }

    public override void Exit(Drone drone) { }
}

public class D_Attackstate : DroneState
{
    public override void Start(Drone drone) 
    {
        drone.SetDroneActive(true);
        drone.StartCoroutine("Explosion_timer");
        Debug.Log("Attack State ½ÃÀÛ");
    }
    public override void Update(Drone drone) 
    {
        drone.transform.position = Vector3.MoveTowards(drone.transform.position, drone.Player_trans.position , drone.D_speed * Time.deltaTime);  
    }
    public override void Exit(Drone drone) { }
}

