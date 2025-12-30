using Unity.Burst.Intrinsics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // stats은 Scriptable Object화 되었음
    // 각 총알에 맞게끔 Scriptable Object를 만들어 사용할 것
    [SerializeField] protected Transform parent;
    [SerializeField] protected Rigidbody2D rigid;
    [SerializeField] protected BulletStat stats;
    
    protected float timer;

    protected bool enhance;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        LifeTime();
    }

    protected void LifeTime()
    {
        // 터널링 현상으로 인한 풀 복귀 불능 제어
        // 일정시간 후 풀로 복귀
        timer += Time.deltaTime;
        if (timer >= stats.lifeTime)
        {
            transform.SetParent(parent);
            gameObject.SetActive(false);
        }
    }

    public virtual void Init(Vector2 dir, Vector3 pos, bool enhanced = false)
    {
        // Bullet 속성의 투사체 초기화
        parent = GetComponentInParent<Transform>();
        timer = 0;
        enhance = enhanced;
        transform.position = pos;
        gameObject.SetActive(true);
        Shoot(dir);
    }

    public virtual void Shoot(Vector2 dir)
    {
        // 발사
        transform.SetParent(null);
        rigid.linearVelocity = dir * stats.speed;
    }

    public virtual void Rotating(Vector2 dir)
    {
        // 방향에 맞게 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        gameObject.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    protected virtual void Triggered(Collider2D collision)
    {
        // 각 레이어에 맞게 작용하게끔 작성할 것
        // 데미지 함수 매개변수 작성시 enhance 여부를 판단하는 로직을 넣을 것;
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
        float enhancing = enhance ? 2 : 1;
        float damage = stats.damage * enhancing;
        switch(collision.gameObject.layer)
        {
            case (int)Layers.terrain:
                transform.SetParent(parent);
                gameObject.SetActive(false);
                break;
            case (int)Layers.enviroment:
                collision.GetComponent<IDamageable>()?.TakeDamage(damage);
                break;
            case (int)Layers.enemy:
                transform.SetParent(parent);
                collision.GetComponent<IDamageable>()?.TakeDamage(damage);
                gameObject.SetActive(false);
                break;
            case (int)Layers.boss:
                transform.SetParent(parent);
                collision.GetComponent<IDamageable>()?.TakeDamage(damage);
                gameObject.SetActive(false);
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

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        Triggered(collision);
    }
}
