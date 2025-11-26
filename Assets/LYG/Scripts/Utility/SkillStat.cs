using UnityEngine;

[CreateAssetMenu(fileName = "SkillStat", menuName = "Scriptable Objects/SkillStat")]
public class SkillStat : ScriptableObject
{
    public float staminaCost;
    public float coolTime;
    public int attackCount;
    public int bulletCost;
}
