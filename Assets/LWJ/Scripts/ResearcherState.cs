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

    private float wallCheckDistance = 0.5f; // 전방 벽 감지 거리
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
            Movedistance *= -1; // 방향 반전
            FlipResearcher(researcher, Movedistance); // 방향 전환 시 캐릭터 스프라이트 뒤집기
        }

        // 3. 이동 실행
        Vector3 movement = Vector3.right * Movedistance * R_Speed * Time.deltaTime;
        researcher.transform.position += movement;

    }

    private bool CheckForObstacle(Researcher researcher)
    {
        // 현재 이동 방향으로 벽을 확인
        Vector2 checkDirection = (Movedistance > 0) ? Vector2.right : Vector2.left;

        // 레이캐스트 발사: 연구원의 중심 위치에서 전방으로 발사
        RaycastHit2D hit = Physics2D.Raycast(
            researcher.transform.position,
            checkDirection,
            wallCheckDistance,
            researcher.groundLayer // 벽/땅 레이어 마스크 사용
        );

        Debug.DrawRay(researcher.transform.position, checkDirection * wallCheckDistance, (hit.collider != null) ? Color.red : Color.green); 

        return hit.collider != null;
    }

    private bool CheckForLedge(Researcher researcher)
    {
        // 연구원의 현재 이동 방향 쪽 발밑을 확인
        Vector3 footPosition = researcher.transform.position;
        // 캐릭터의 좌우 끝점에서 발사 (offset 조정 필요)
        footPosition.x += Movedistance * 0.3f;

        // 레이캐스트 발사: 발밑 위치에서 아래로 발사
        RaycastHit2D hit = Physics2D.Raycast(
            footPosition,
            Vector2.down,
            groundCheckDistance,
            researcher.groundLayer // 땅 레이어 마스크 사용
        );

        Debug.DrawRay(footPosition, Vector2.down * groundCheckDistance, (hit.collider != null) ? Color.yellow : Color.cyan); 

        // Raycast 결과가 null이면 (땅을 못 찾으면) 낭떠러지이므로 true 반환
        return hit.collider == null;
    }
    public override void Exit(Researcher researcher)
    {
        Debug.Log("Researcher Idle State 종료");
    }

    private void FlipResearcher(Researcher researcher, float direction)
    {
 
        Vector3 currentScale = researcher.transform.localScale;

    
        if (direction > 0 && currentScale.x < 0)
        {
            researcher.transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

        else if (direction < 0 && currentScale.x > 0)
        {
            researcher.transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
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

