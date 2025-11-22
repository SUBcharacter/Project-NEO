using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] LayerMask attackMask;

    [SerializeField] float speed;
    float timer;
    float lifeTime = 3.5f;
    
    [SerializeField] int damage;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    public void Init(Vector2 dir, Vector3 pos)
    {
        timer = 0;
        transform.position = pos;
        gameObject.SetActive(true); 
        rigid.linearVelocity = dir * speed;
    }

    void Triggered(Collider2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & attackMask) == 0)
            return;

        gameObject.SetActive(false);
    }
}
