using System.Collections;
using UnityEngine;

public class BulletFX : BaseProjectile
{
    private void Start()
    {
        damage = 15f;
        //StartCoroutine(DestroyAfterAnimation());
    }
    IEnumerator DestroyAfterAnimation()
    {
        yield return CoroutineCasher.Wait(0.1f);
        Destroy(gameObject);
    }
    protected override void OnHitBoss(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
    protected override void OnHitBorder(Collider2D collision) { }
    protected override void OnHitTerrain(Collider2D collision) { }
    protected override void OnHitPlayer(Collider2D collision) { }
}
