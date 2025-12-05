using UnityEngine;
using UnityEngine.EventSystems;

public class R_Bullet : Bullet
{
    [SerializeField] LayerMask attackMask;

    protected override void Awake()
    {
        base.Awake();
       
        stats.attackable = attackMask;
        stats.damage = 5f;
        stats.speed = 15f;
        stats.lifeTime = 3f;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Init(Vector2 dir, Vector3 pos)
    {
        base.Init(dir, pos);    
        Rotating(dir);
    }

    protected override void Triggered(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
        switch(collision.gameObject.layer)
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
                transform.SetParent(parent);
                gameObject.SetActive(false);
                break;
            case (int)Layers.border:
                break;
            case (int)Layers.invincible:
                break;
        }

    }

}
