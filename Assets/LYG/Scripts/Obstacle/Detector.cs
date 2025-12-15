using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] LayerMask detectable;

    [SerializeField] float radius;

    public Transform Detect()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, detectable);

        if (hit != null)
            return hit.transform;

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
