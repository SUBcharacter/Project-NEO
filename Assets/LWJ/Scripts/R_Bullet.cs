using UnityEngine;
using UnityEngine.EventSystems;

public class R_Bullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] LayerMask attackMask;
    private Vector2 moveDirection;
    [SerializeField] private float bulletSpeed = 200f;
    private float lifeTime = 5f;

    [SerializeField] int damage;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        transform.Translate(moveDirection * bulletSpeed * Time.deltaTime, Space.World);

    }

    public void Init(Vector2 direction,float speed)
    {
        moveDirection = direction.normalized;
        bulletSpeed = speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifeTime);

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
