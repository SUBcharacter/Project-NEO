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
    float D_Speed = 2f;


    private float wallCheckDistance = 0.5f; // 전방 벽 감지 거리
    private float groundCheckDistance = 0.8f;
    public override void Start(Drone drone) 
    {

    }
    public override void Update(Drone drone)
    {
        if(drone.sightRange != null && drone.sightRange.IsPlayerInSight)
        {
            drone.ChangeState(drone.droneStates[1]);
            return;
        }

        if (CheckForObstacle(drone) )
        {
            drone.Movedistance *= -1; // 방향 반전
            drone.FlipDrone(drone, drone.Movedistance); // 방향 전환 시 캐릭터 스프라이트 뒤집기
        }

        // 3. 이동 실행
        Vector3 movement = Vector3.right * drone.Movedistance * D_Speed * Time.deltaTime;
        drone.transform.position += movement;
    }

    public override void Exit(Drone drone) { }
    private bool CheckForObstacle(Drone drone)
    {
        // 현재 이동 방향으로 벽을 확인
        Vector2 checkDirection = (drone.Movedistance > 0) ? Vector2.right : Vector2.left;

        // 레이캐스트 발사: 연구원의 중심 위치에서 전방으로 발사
        RaycastHit2D hit = Physics2D.Raycast(
            drone.transform.position,
            checkDirection,
            wallCheckDistance,
            drone.groundLayer // 벽/땅 레이어 마스크 사용
        );

        Debug.DrawRay(drone.transform.position, checkDirection * wallCheckDistance, (hit.collider != null) ? Color.red : Color.green);

        return hit.collider != null;
    }



}

public class D_Summonstate : DroneState
{
    public override void Start(Drone drone)
    {
        Debug.Log("Summon State 시작");
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

    float horizontalDirection = 1f;
    public override void Start(Drone drone) 
    {
        drone.SetDroneActive(true);
        drone.StartCoroutine("Explosion_timer");
        Debug.Log("Attack State 시작");
    }
    public override void Update(Drone drone) 
    {
        drone.transform.position = Vector3.MoveTowards(drone.transform.position, drone.Player_trans.position , drone.D_speed * Time.deltaTime);
        float directionX = drone.Player_trans.position.x - drone.transform.position.x;

        // X축 차이의 부호만 필요 (양수: 오른쪽, 음수: 왼쪽)
       
        if (directionX > 0)
        {
            horizontalDirection = 1f;
        }
        else if (directionX < 0)
        {
            horizontalDirection = -1f;
        }
     
            
            drone.FlipDrone(drone, horizontalDirection);
        
    }
    public override void Exit(Drone drone) { }
    }

