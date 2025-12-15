using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillStat", menuName = "Scriptable Objects/SkillStat")]
public class SkillStat : ScriptableObject
{
    [Header("Common Parameter")]
    public float staminaCost;
    public float coolTime;
    public int bulletCost;
    public int damage;

    [Header("Phantom Blade Parameter")]
    public int attackCount;

    [Header("Charge Attack Parameter")]
    public float chargeSpeed;
    public float chargeAccel;
    public float knockBackForce;

    [Header("Auto Targeting Parameter")] 
    public LayerMask scanable;
    public float scanRadius;
    public float scanTime;

    [Header("Flash Blade")]
    public float attackDistance;
}
