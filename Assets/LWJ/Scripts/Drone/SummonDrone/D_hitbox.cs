using UnityEngine;

public class D_hitbox : HitBox
{
    SummonDrone summondrone;
    protected override void Awake()
    {
        col = GetComponent<Collider2D>();
        summondrone = GetComponent<SummonDrone>();
    }

    public override void Init(bool enhanced = false)
    {
        triggered = false;
    }

    protected override void Triggered(GameObject collision)
    {
        if(summondrone.currentStates is SD_Attackstate == false) return;

        if (((1 << collision.gameObject.layer) & stats.attackable) == 0) return;


        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            Debug.Log("플레이어에게 데미지 입힘");
            player.TakeDamage(stats.damage);

        }
        else
        {
            Debug.Log("플레이어를 찾지 못햇습니다");
        }


    }
}
