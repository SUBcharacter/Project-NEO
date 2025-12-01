using UnityEngine;

public class BulletOrb : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Rigidbody2D rigid;

    [SerializeField] Vector2 dir;
    [SerializeField] LayerMask absorbable;

    [SerializeField] float speed;
    [SerializeField] float timer;
    [SerializeField] float lifeTime;

    [SerializeField] int amount;

    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
        rigid = GetComponent<Rigidbody2D>();
        timer = lifeTime;
        amount = Random.Range(1, 4);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

        rigid.AddForce(dir * speed , ForceMode2D.Impulse);

        if(timer <= 0)
        {
            player.GetBullet(amount);
            gameObject.SetActive(false);
        }
    }

    public void Init(Vector3 pos)
    {
        transform.position = pos;
        timer = lifeTime;
        amount = Random.Range(1, 4);
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & absorbable) == 0)
                return;

        collision.GetComponent<Player>().GetBullet(amount);
        gameObject.SetActive(false);
    }
}
