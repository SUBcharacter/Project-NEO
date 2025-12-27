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
        float damage = stats.damage;
        if (summondrone.currentStates is SD_Attackstate == false) return;

        if (((1 << collision.gameObject.layer) & stats.attackable) == 0) return;

        switch (collision.layer)
        {
            case (int)Layers.terrain:
                triggered = true;
                Debug.Log("충돌");
                break;
            case (int)Layers.enviroment:
                triggered = true;
                Debug.Log("충돌");
                break;
            case (int)Layers.enemy:
                triggered = true;
                break;
            case (int)Layers.player:
                Debug.Log("플레이어 충돌");
                collision.GetComponent<IDamageable>().TakeDamage(damage);
                triggered = true;
                break;
            case (int)Layers.border:
                triggered = true;
                break;
            case (int)Layers.invincible:
                triggered = true;
                break;
        }

    }
}
