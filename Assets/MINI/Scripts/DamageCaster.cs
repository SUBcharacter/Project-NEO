using UnityEngine;

public class DamageCaster : BaseProjectile
{
    [SerializeField] private bool oneHitEnable = true;
    private readonly System.Collections.Generic.HashSet<GameObject> hitObjects = new();

    private void OnEnable()
    {
        if (oneHitEnable)
        {
            hitObjects.Clear();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (oneHitEnable)
        {
            if (hitObjects.Contains(collision.gameObject)) return;

            hitObjects.Add(collision.gameObject);
        }
        base.OnTriggerEnter2D(collision);
    }
    protected override void OnHitBoss(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
    protected override void OnHitBorder(Collider2D collision) { }
    protected override void OnHitTerrain(Collider2D collision) { }
    protected override void OnHitPlayer(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
