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

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.enabled = false;     // 초반은 비활성화

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        int objLayer = collision.collider.gameObject.layer;

        if (((1 << objLayer) & checkLayer) == 0) return;

    }

    public void Launch(Vector2 dir, float targetPower, float arcPower)
    {
        rb.gravityScale = gravity;

        float vx = dir.x * targetPower;

        float vy = arcPower;

        Vector2 targetPos = dir - (Vector2)transform.position;

        rb.linearVelocity = new Vector2(vx, vy);

    }
}
