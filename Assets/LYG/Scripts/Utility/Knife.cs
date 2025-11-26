using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Knife : Bullet
{
    [SerializeField] Vector2 attackDir;
    Camera cam;

    [SerializeField] bool attacking;

    protected override void Awake()
    {
        base.Awake();
        attacking = false;
        cam = Camera.main;
    }

    protected override void Update()
    {
        if(!attacking)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            attackDir = (mousePos - (Vector2)transform.position).normalized;
            Rotating(attackDir);
        }
        else
        {
            LifeTime();
        }
    }

    public override void Init(Vector2 dir, Vector3 pos)
    {
        parent = GetComponentInParent<Transform>();
        timer = 0;
        attacking = false;
        transform.position = pos;
        gameObject.SetActive(true);
        
    }

    public override void Shoot(Vector2 _)
    {
        attacking = true;   
        transform.SetParent(null);
        rigid.linearVelocity = attackDir * stats.speed;
    }

    protected override void Triggered(Collider2D collision)
    {
        // 각 레이어에 맞게 작용하게끔 작성할 것
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
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
