using UnityEngine;

[CreateAssetMenu(fileName = "BulletStat", menuName = "Scriptable Objects/BulletStat")]
public class BulletStat : ScriptableObject
{
    public LayerMask attackable;

    public float speed;
    public float damage;
    public float lifeTime;
}
