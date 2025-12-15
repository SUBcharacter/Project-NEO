using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class DebrisProjectile : MonoBehaviour
{
    [Header("잔해 오브젝트 Setting")]
    [SerializeField] private LayerMask checkLayer;
    public float lifetime = 5f;     
    private float gravity = 3f;     // 포물선으로 던질 때의 중력 값

    private Rigidbody2D rb;
    private Collider2D col;

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



    public void Launch(Vector2 direction, float forwardPower, float arcPower)
    {
        rb.gravityScale = gravity;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 기본 진행 방향
        Vector2 forward = direction * forwardPower;

        // 위로 들어올리는 힘
        Vector2 lift = Vector2.up * arcPower;

        rb.linearVelocity = forward + lift;

        col.enabled = true;
        Destroy(gameObject, lifetime);
    }




}
