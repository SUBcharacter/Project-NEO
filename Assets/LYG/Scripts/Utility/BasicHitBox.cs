using UnityEngine;

public class BasicHitBox : HitBox
{
    protected override void Awake()
    {
        triggered = false;
        col = GetComponent<Collider2D>();
    }

    public override void Init()
    {        
        gameObject.SetActive(true);
    }
    private void HandleHit(GameObject target)
    {
        
        if (((1 << target.layer) & stats.attackable) == 0) return;

        // 레이어별 분기
        switch (target.layer)
        {
            case (int)Layers.terrain:
                triggered = true;                
                break;
            case (int)Layers.player:                
                if (target.TryGetComponent<IDamageable>(out var victim))    // 이거는 내가 쓸 용도로 한번 구현해본거
                {
                    victim.TakeDamage(stats.damage);
                }
                triggered = true;
                break;
            case (int)Layers.border:
                triggered = true;
                break;

        // 필요 시 다른 케이스 추가 하면 좋을 듯?
        }
    }

    // 12/7 하나로 묶어서 간략화 시킴
    protected override void Triggered(Collider2D collision)
    {
        HandleHit(collision.gameObject);        
    }

    protected override void Triggered(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }
}
