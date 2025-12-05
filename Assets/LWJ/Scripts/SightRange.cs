using UnityEngine;

public class SightRange : MonoBehaviour
{
 
    [Header("FOV Settings")]
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

    private void FindTargetsInFOV()
    {

        float dirMultiplier = Mathf.Sign(transform.localScale.x); 
        Vector2 fovDirection = Vector2.right * dirMultiplier;
        IsPlayerInSight = false; 
        PlayerInSight = null;

        //플레이어가 시야 반경 내에 있다면 플레이어를 향해 레이캐스트
        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetLayer);

        foreach (Collider2D targetCollider in targetsInRadius)
        {
            Transform target = targetCollider.transform;
            Vector2 dirToTarget = (target.position - transform.position).normalized;

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
