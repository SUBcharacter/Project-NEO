using UnityEngine;

[CreateAssetMenu(fileName = "BisiliStat", menuName = "Scriptable Objects/BisiliStat")]
public class BisiliStat : ScriptableObject
{
    public float maxHealth;
    public float speed;
    public float swaySpeed;
    public float returnSpeed;
    public float waitTime;
    public float attackDistance;
    public float attackDuration;
    public float hitDuration;
}
