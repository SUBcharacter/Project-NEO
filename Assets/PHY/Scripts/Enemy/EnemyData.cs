using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("이동 관련")]
    public float moveSpeed = 1.5f;
    public float moveDistance = 2.5f;

    [Header("체력 관련")]
    public int maxHits = 4;

    [Header("총알 관련")]
    public int bulletCount = 4;
    public float bulletInterval = 0.15f;

    [Header("공격 관련")]
    public float fireCooldown = 1f;
    public float readyToFireTime = 0.5f;
}
