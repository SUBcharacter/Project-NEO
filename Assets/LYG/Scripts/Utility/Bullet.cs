using UnityEngine;

public class Bullet : MonoBehaviour
{
    // stats은 Scriptable Object화 되었음
    // 각 총알에 맞게끔 Scriptable Object를 만들어 사용할 것
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] BulletStat stats;
    
    float timer;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= stats.lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    public void Init(Vector2 dir, Vector3 pos)
    {
        timer = 0;
        transform.position = pos;
        gameObject.SetActive(true); 
        rigid.linearVelocity = dir * stats.speed;
    }

    void Triggered(Collider2D collision)
    {
        // 각 레이어에 맞게 작용하게끔 작성할 것
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
        switch(collision.gameObject.layer)
        {
            case (int)Layers.terrain:
                break;
            case (int)Layers.enviroment:
                break;
            case (int)Layers.enemy:
                break;
            case (int)Layers.player:
                break;
            case (int)Layers.border:
                gameObject.SetActive(false);
                break;
            case (int)Layers.invincible:
                gameObject.SetActive(false);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Triggered(collision);
    }
}
