using UnityEngine;


// 이 스크립트를 Enemy오브젝트에 붙여도 상관x
// 이대로도 좋지만 아직 벽뒤에 있을때 플레이어를 인식하는 문제는 고치지 못함
// 인식된 플레이어 방향으로 레이캐스트를 쏴서 벽 or 지형에 가로 막히면 detectedPlayer Transform을 null로 만드는 방법도 좋을 듯?(개인 취향)
// 그리고 현재 구조상 DetectionSensor에서 LongDiEnemy가 detectedPlayer Transform을 참조하는 형식으로 되어 있지만
// 함수를 통해서 Transform을 반환하는 형식으로 가면 상태 로직짜기가 훨씬 수월할 것
public class DetectionSensor : MonoBehaviour
{
    [Header("Detection Setting")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] public LayerMask obstacleLayer;

    public bool IsPlayerDetected { get; private set; }
    public bool IsPlayerInAttackRange { get; private set; }
    public Transform detectedPlayer { get; private set; }

    public Transform GetDetectedPlayer() => detectedPlayer;

    // Update is called once per frame
    void Update()
    {
        // 이 부분을 함수로 독립 시킨 다음, Transform을 반환하는 형식
        // LongDiEnemy에서는 단순히 DetectionSensor를 참조해서 함수만 실행시켜주면 될일
        detectedPlayer = null;
        IsPlayerDetected = false;
        IsPlayerInAttackRange = false;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit == null) return;


        Vector2 direction = (hit.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, hit.transform.position);

        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        if (raycastHit && raycastHit.collider.gameObject != hit.gameObject)
            return;

        detectedPlayer = hit.transform;
        IsPlayerDetected = true;

        IsPlayerInAttackRange = distance <= attackRadius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
