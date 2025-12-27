using UnityEngine;

public class PunchHitBox : HitBox
{

    protected override void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;   // 충돌 감지 전용
        col.enabled = false;    // 패턴 시작될 때까지 비활성
    }

    public override void Init(bool enhanced = false)
    {
        Debug.Log($"PunchHitBox Pos: {transform.position}");

        triggered = true;
        col.enabled = true;
    }

    public void Disable()
    {
        triggered = false;
        col.enabled = false;
    }

    protected override void Triggered(GameObject collision)
    {
        if (!triggered) return;

        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;

        if (collision.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(stats.damage);
        }
    }

  
}
