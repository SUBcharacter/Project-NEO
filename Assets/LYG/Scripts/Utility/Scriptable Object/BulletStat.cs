using UnityEngine;

[CreateAssetMenu(fileName = "BulletStat", menuName = "Scriptable Objects/BulletStat")]
public class BulletStat : ScriptableObject
{
    // 투사체(Bullet 계열) 스탯

    // 공격 가능 레이어
    public LayerMask attackable;

    // 투사체 속도
    public float speed;

    // 투사체 데미지
    public float damage;

    // 라이프타임
    public float lifeTime;
}
