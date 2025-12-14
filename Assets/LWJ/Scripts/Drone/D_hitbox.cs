using UnityEngine;

public class D_hitbox : HitBox
{
    protected override void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    public override void Init(bool enhanced = false)
    {
        triggered = false;
    }

    protected override void Triggered(Collision2D collision)
    {

    }

    protected override void Triggered(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0) return;


        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            Debug.Log("플레이어에게 데미지 입힘");
            player.Hit((int)stats.damage);

        }
        else
        {
            Debug.Log("플레이어를 찾지 못햇습니다");
        }


    }
}
