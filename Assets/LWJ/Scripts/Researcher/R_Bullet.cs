using UnityEngine;
using UnityEngine.EventSystems;

public class R_Bullet : Bullet
{

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Init(Vector2 dir, Vector3 pos, bool enhanced = false)
    {
        parent = GetComponentInParent<Transform>();
        transform.SetParent(parent);
        rigid.simulated = false;
        timer = 0;
        transform.localPosition = pos;
        gameObject.SetActive(true);
        base.Init(dir, pos);
        Rotating(dir);
    }

    public override void Shoot(Vector2 dir)
    {
        rigid.simulated = true;
        transform.SetParent(null);
        rigid.linearVelocity = dir * stats.speed;
    }
    protected override void Triggered(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0) return;

        Player player = collision.GetComponent<Player>();

        switch (collision.gameObject.layer)
        {
            case (int)Layers.terrain:
                transform.SetParent(parent);
                gameObject.SetActive(false);
                break;
            case (int)Layers.enviroment:
                break;
            case (int)Layers.enemy:
                break;
            case (int)Layers.player:
                player.Hit((int)stats.damage);
                transform.SetParent(parent);
                gameObject.SetActive(false);
                break;
            case (int)Layers.border:
                transform.SetParent(parent);
                gameObject.SetActive(false);
                break;
            case (int)Layers.invincible:
                transform.SetParent(parent);
                gameObject.SetActive(false);
                break;
        }
    }
}
