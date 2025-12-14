using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("이동 관련")]
    public float moveSpeed;
    public float moveDistance;

    [Header("체력 관련")]
    public float MaxHp;

    [Header("총알 관련")]
    public int bulletcount;

    [Header("공격 관련")]
    public float fireCooldown;
    public float Damage;
}
