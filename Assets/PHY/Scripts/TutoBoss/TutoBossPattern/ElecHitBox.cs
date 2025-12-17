using UnityEngine;

public class ElecHitBox : HitBox
{
    protected override void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }
    public override void Init()
    {
        triggered = true;
        col.enabled = true;
    }

    protected override void Triggered(Collision2D collision)
    {
        
    }

    protected override void Triggered(Collider2D collision)
    {
        if (!triggered) return;

        if(((1 <<  collision.gameObject.layer) & stats.attackable) == 0)
        {
            return;
        }

        Debug.Log($"전기충격 받은 대상 :{collision.name} ");

        if(collision.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(stats.damage);
        }

        // 경직은 나중에 
    }

   
}
