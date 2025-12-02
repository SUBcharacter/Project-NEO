using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HitBoxStat stats;
    public Collider2D col;

    public bool triggered;
    protected virtual void Awake()
    {
        triggered = false;
        col = GetComponent<Collider2D>();
    }

    public virtual void Init()
    {
        triggered = false;
        gameObject.SetActive(true);
    }

    protected virtual void Triggered(Collision2D collision)
    {
        // 각 레이어에 맞게 작용하게끔 작성할 것
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

    protected virtual void Triggered(Collider2D collision)
    {
        // 각 레이어에 맞게 작용하게끔 작성할 것
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

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        Triggered(collision);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        Triggered(collision);
    }
}
