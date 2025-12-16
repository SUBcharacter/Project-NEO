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
        base.Init(dir, pos);
        Rotating(dir);
    }

    protected override void Triggered(Collider2D collision)
    {

        if (((1 << collision.gameObject.layer) & stats.attackable) == 0) return; 

        if (collision.gameObject.layer == (int)Layers.player)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // Player 데미지 처리
                player.Hit((int)stats.damage); // 단순 데미지 적용

                transform.SetParent(parent);
                gameObject.SetActive(false);
                return; // Player 처리 후 기본 Bullet 로직 호출 방지
            }
        }
        base.Triggered(collision);
    }
}
