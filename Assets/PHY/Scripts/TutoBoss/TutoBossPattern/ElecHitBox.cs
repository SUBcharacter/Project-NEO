using UnityEngine;

/// <summary>
/// [ElecHitBox]
/// - 전기충격파 전용 히트박스
/// - 패턴 실행 시 Init()을 통해 활성화됨
/// - Trigger 충돌 시 IDamageable 대상에게 데미지 처리
/// </summary>
public class ElecHitBox : HitBox
{
    protected override void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;   // 충돌 감지 전용
        col.enabled = false;    // 패턴 시작될 때까지 비활성
    }

    public override void Init()
    {
        triggered = true;       // HitBox 동작 플래그
        col.enabled = true;     // 실제로 충돌 판정 시작
    }

    protected override void Triggered(Collision2D collision)
    {
        // 2D Trigger 충돌이므로 여기서는 사용하지 않음
    }

    protected override void Triggered(Collider2D collision)
    {
        if (!triggered) return; // 활성화 안됐으면 무시

        // 공격 가능한 레이어인지 판정
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;

        Debug.Log($"전기충격 받은 대상 : {collision.name}");

        // IDamageable 있으면 데미지 적용
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(stats.damage);
        }

        // TODO: 경직 처리는 추후 추가
    }
}
