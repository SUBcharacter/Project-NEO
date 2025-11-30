using UnityEngine;

public class SightRange : MonoBehaviour
{
    // Inspector에서 설정할 시야 관련 변수들
    [Header("FOV Settings")]
    [SerializeField] private float viewRadius = 5f; // 시야 반경
    [Range(0, 360)] // 슬라이더 형태로 0~360 범위 설정
    [SerializeField] private float viewAngle = 90f; // 시야각 (0~360도)

    // 플레이어와 장애물 레이어를 Inspector에서 설정
    [SerializeField] private LayerMask targetLayer; // 플레이어 레이어
    [SerializeField] private LayerMask obstacleLayer; // 장애물 레이어 

    // 플레이어 감지 여부
    public bool IsPlayerInSight { get; private set; } = false;

    // 플레이어가 감지되면 그 플레이어의 트랜스폼을 저장
    public Transform PlayerInSight { get; private set; }

    // 매 프레임 시야 내 플레이어 찾기
    void Update()
    {
        FindTargetsInFOV();
    }

    private void FindTargetsInFOV()
    {
        IsPlayerInSight = false; // 매 프레임 초기화
        PlayerInSight = null;

        // 시야의 기준 방향을 왼쪽
        Vector2 fovDirection = Vector2.left;

        //플레이어가 시야 반경 내에 있다면 그 플레이어를 향해 레이캐스트
        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetLayer);

        foreach (Collider2D targetCollider in targetsInRadius)
        {
            Transform target = targetCollider.transform;
            Vector2 dirToTarget = (target.position - transform.position).normalized;

            // 시야각 내에 있는지 확인
            if (Vector2.Angle(fovDirection, dirToTarget) < viewAngle / 2)
            {
                // 장애물이 플레이어를 가리고 있는지 확인
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dirToTarget, viewRadius, obstacleLayer | targetLayer);

                bool obstacleFound = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (((1 << hit.collider.gameObject.layer) & obstacleLayer) > 0)
                    {
                      
                        obstacleFound = true;
                        break;
                    }
                    if (((1 << hit.collider.gameObject.layer) & targetLayer) > 0)
                    {
                      
                        obstacleFound = false; 
                        break;
                    }
                }

                if (!obstacleFound)
                {
                    IsPlayerInSight = true;
                    PlayerInSight = target;
                    return;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // 시야 반경 원
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.red; // 시야각 선

   
        float baseAngle = 180f;

    
        Vector3 viewAngleA = DirFromAngle(baseAngle - viewAngle / 2);
        Vector3 viewAngleB = DirFromAngle(baseAngle + viewAngle / 2);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        // 플레이어를 감지시 선
        if (IsPlayerInSight && PlayerInSight != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, PlayerInSight.position);
        }
    }


    public Vector3 DirFromAngle(float angleInDegrees)
    {
        float angleRad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
    }
}
