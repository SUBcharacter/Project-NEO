using UnityEditor.Searcher;
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
        triggered = false;
        gameObject.SetActive(true);
    }

    protected override void Triggered(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
        Researcher researcher = collision.gameObject.GetComponent<Researcher>();
        switch (collision.gameObject.layer)
        {
            case (int)Layers.terrain:
                triggered = true;
                Debug.Log("面倒");
                break;
            case (int)Layers.enviroment:
                triggered = true;
                Debug.Log("面倒");
                break;
            case (int)Layers.enemy:
                researcher.TakeDamage(stats.damage);
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

        Researcher researcher = collision.gameObject.GetComponent<Researcher>();

        switch (collision.gameObject.layer)
        {
            case (int)Layers.terrain:
                triggered = true;
                Debug.Log("面倒");
                break;
            case (int)Layers.enviroment:
                triggered = true;
                Debug.Log("面倒");
                break;
            case (int)Layers.enemy:
                researcher.TakeDamage(stats.damage);    
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
