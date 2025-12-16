using UnityEngine;

public class ElecShockWave : MonoBehaviour
{
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] HitBox shockHitBox; 
    public float lifeTime = 4f;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    private void Start()
    {
        col.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!col.enabled) return;

        Debug.Log($"충격 받음: {collision.name}");

        int objLayer = collision.gameObject.layer;

        if (((1 << objLayer) & damageLayer) == 0)
        {
            Debug.Log("충격받았지만 damageLayer에 포함 안됨");
            return;
        }

        Destroy(gameObject, lifeTime);
    }

}
