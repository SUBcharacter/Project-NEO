using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public bool isPlayerInAttackRange { get; private set; }
    public Transform target { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isPlayerInAttackRange = true;
            target = collision.transform;
        }
    }

}
