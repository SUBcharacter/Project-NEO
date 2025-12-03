using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] LayerMask detectable;

    public Transform Detect()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 8, detectable);

        if (hit != null)
            return hit.transform;

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 8);
    }
}
