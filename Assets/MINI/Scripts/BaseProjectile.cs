using UnityEngine;
public enum LayerType
{
    Terrain = 3,
    Environment = 6,
    Enemy = 7,
    Player = 8,
    Border = 9,
    Invincible = 10,
    Boss = 11
}

public abstract class BaseProjectile : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] protected float damage = 10;
    [SerializeField] protected LayerMask targetLayer;

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
            return;

        HandleCollision(collision);
    }
    protected virtual void HandleCollision(Collider2D collision)
    {
        int layer = collision.gameObject.layer;

        if (layer == (int)LayerType.Terrain) OnHitTerrain(collision);
        else if (layer == (int)LayerType.Player) OnHitPlayer(collision);
        else if (layer == (int)LayerType.Boss) OnHitBoss(collision);
        else if (layer == (int)LayerType.Border) OnHitBorder(collision);
    }
    protected virtual void OnHitBoss(Collider2D collision)
    {
        ApplyDamage(collision);
        Destroy(gameObject);
    }
    protected virtual void OnHitPlayer(Collider2D collision)
    {
        ApplyDamage(collision);
        // 플레이어 피격 시 투사체 파괴 여부는 기획에 따라 다르게 할거임 (관통 등)
        Destroy(gameObject);
    }
    protected void ApplyDamage(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
    protected virtual void OnHitTerrain(Collider2D collision)
    {
        Destroy(gameObject);
    }
    protected virtual void OnHitBorder(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
