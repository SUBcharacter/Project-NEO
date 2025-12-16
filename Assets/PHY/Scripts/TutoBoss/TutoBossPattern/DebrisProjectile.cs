using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class DebrisProjectile : MonoBehaviour
{
    [Header("잔해 오브젝트 Setting")]
    [SerializeField] private LayerMask checkLayer;
    public float lifetime = 5f;     

    public Rigidbody2D rb;
    public Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.enabled = false;     // 초반은 비활성화

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"충돌 감지됨: {collision.collider.name}");

        int objLayer = collision.collider.gameObject.layer;

        if (((1 << objLayer) & checkLayer) == 0)
        {
            Debug.Log("충돌했지만 checkLayer에 포함 안됨");
            return;
        }

        Destroy(gameObject);
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        rb.linearVelocity = direction * speed;


        col.enabled = true;
        Destroy(gameObject, lifetime);
    }


}
