using UnityEngine;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

public class AimRange : MonoBehaviour
{
    [SerializeField] private float viewRadius = 5f;
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90f;

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;

    public bool IsPlayerInSight { get; private set; } = false;

    public Transform PlayerInSight { get; private set; }

    void Update()
    {
        FindTargetsInFOV();
    }

    void FindTargetsInFOV()
    {
        float dirMultiplier = Mathf.Sign(transform.localScale.x); // 방향에 따른 조정 : 왼쪽 : 음수 오른쪽 : 양수
        Vector2 fovDirection = Vector2.right * dirMultiplier; // 시야 방향 설정
        IsPlayerInSight = false;
        PlayerInSight = null;

        //플레이어가 시야 반경 내에 있다면 플레이어를 향해 레이캐스트
        Collider2D targetcollider = Physics2D.OverlapCircle(transform.position, viewRadius, targetLayer);

        if (targetcollider == null)
        {
            return; // 목표물이 없으면 함수 종료
        }

        Transform target = targetcollider.transform;// 타겟의 Transform 가져오기
        Vector2 dirToTarget = (target.position - transform.position).normalized; // 타겟 방향 계산

        if (Vector2.Angle(fovDirection, dirToTarget) < viewAngle / 2) // 시야각 내에 있는지 확인
        {
            //타겟까지의 거리 계산
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            // 장애물이 플레이어를 가리고 있는지 확인
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleLayer);

            if (hit.collider == null)
            {
                // 시야 확보 및 장애물 없을 시
                IsPlayerInSight = true;
                PlayerInSight = target;
            }


        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        
        Gizmos.color = Color.red;
        
        
        float dirMultiplier = Mathf.Sign(transform.localScale.x);
        float baseAngle = (dirMultiplier > 0) ? 0f : 180f;
        
        Vector3 viewAngleA = DirFromAngle(baseAngle - viewAngle / 2);
        Vector3 viewAngleB = DirFromAngle(baseAngle + viewAngle / 2);
        
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
        
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
