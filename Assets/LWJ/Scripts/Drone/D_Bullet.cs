using UnityEngine;

public class D_Bullet : Bullet
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
        enhance = enhanced;
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
        float enhancing = enhance ? 2 : 1;
        rigid.simulated = true;
        transform.SetParent(null);
        rigid.linearVelocity = dir * stats.speed * enhancing;
    }
    public override void Rotating(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    protected override void Triggered(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0) return;

        Player player = collision.GetComponent<Player>();

        float enhancing = enhance ? 2 : 1;
        float damage = stats.damage * enhancing;

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
                player.GetComponent<IDamageable>().TakeDamage(damage);
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
