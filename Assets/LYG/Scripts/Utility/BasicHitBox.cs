using UnityEngine;

public class BasicHitBox : HitBox
{
    bool enhance;

    protected override void Awake()
    {
        triggered = false;
        col = GetComponent<Collider2D>();
    }

    public override void Init(bool enhanced = false)
    {
        enhance = enhanced;
        triggered = false;
        gameObject.SetActive(true);
    }

    protected override void Triggered(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
        // enhance 여부로 데미지 결정 할 것
        switch (collision.gameObject.layer)
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
                collision.gameObject.SetActive(false);
                triggered = true;
                break;
            case (int)Layers.player:
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

    protected override void Triggered(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;

        switch (collision.gameObject.layer)
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
                collision.gameObject.SetActive(false);
                triggered = true;
                break;
            case (int)Layers.player:
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
