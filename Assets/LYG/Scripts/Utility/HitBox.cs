using UnityEngine;

public abstract class HitBox : MonoBehaviour
{
    // 히트박스 특성에 맞게 상속 클래스 작성 후 제작

    [SerializeField] protected Collider2D col;
    [SerializeField] protected HitBoxStat stats;
    [SerializeField] protected bool triggered;

    public HitBoxStat Stats => stats;
    public Collider2D Col { get => col; set => col = value; }
    public bool Trigger => triggered;


    protected abstract void Awake();

    public abstract void Init(bool enhanced = false);

    // OnCollision 이벤트 용
    protected abstract void Triggered(Collision2D collision);

    // OnTriggered 이벤트 용
    protected abstract void Triggered(Collider2D collision);

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        Triggered(collision);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        Triggered(collision);
    }
}
