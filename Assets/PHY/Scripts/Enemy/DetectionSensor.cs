using UnityEngine;

public class DetectionSensor : MonoBehaviour
{
    [Header("Detection Setting")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private LayerMask playerLayer;

    public bool IsPlayerDetected { get; private set; }
    public bool IsPlayerInAttackRange { get; private set; }
    public Transform Player { get; private set; }

    // Update is called once per frame
    void Update()
    {
        Player = null;
        IsPlayerDetected = false;
        IsPlayerInAttackRange = false;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit != null)
        {
            IsPlayerDetected = true;
            Player = hit.transform;

            float distance = Vector2.Distance(transform.position, Player.position);
            IsPlayerInAttackRange = distance <= attackRadius;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
