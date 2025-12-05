using UnityEngine;

public class ExpandingRing : BaseProjectile
{
    [SerializeField] private float ringExpandSpeed = 4f;
    [SerializeField] private float maxScale = 10f;

    private void Start()
    {
        damage = 20f;
    }

    private void Update()
    {
        gameObject.transform.localScale += Vector3.one * ringExpandSpeed * Time.deltaTime;
        if (transform.localScale.x >= maxScale)
        {
            Destroy(gameObject);
        }
    }
    protected override void OnHitTerrain(Collider2D collision)
    {
       
    }
    protected override void OnHitPlayer(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
