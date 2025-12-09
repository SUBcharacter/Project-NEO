using UnityEngine;

public abstract class Orb : MonoBehaviour
{
    [SerializeField] protected Player player;
    [SerializeField] protected Rigidbody2D rigid;

    [SerializeField] protected Vector2 dir;
    [SerializeField] protected LayerMask absorbable;

    [SerializeField] protected float speed;
    [SerializeField] protected float timer;
    [SerializeField] protected float lifeTime;

    [SerializeField] protected float amount;

    protected abstract void Awake();

    protected abstract void Update();

    public abstract void Init(Vector3 pos);

    protected abstract void OnTriggerEnter2D(Collider2D collision);
    
}
