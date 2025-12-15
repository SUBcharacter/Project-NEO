using UnityEngine;

[CreateAssetMenu(fileName = "HitBoxStat", menuName = "Scriptable Objects/HitBoxStat")]
public class HitBoxStat : ScriptableObject
{
    public LayerMask attackable;

    public float damage;
}
