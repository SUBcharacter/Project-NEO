using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Friction Material")]
    public PhysicsMaterial2D noFriction;
    public PhysicsMaterial2D fullFriction;

    [Header("Basic Stats")]
    public int maxHealth;
    public float speed;
    public float dodgeForce;
    public float jumpForce;
    public float airJumpForce;
    public float wallJumpX;
    public float wallJumpY;

    [Header("Cost Stats")]
    public int maxBullet;
    public float maxStamina;
    public float maxOverFlowEnergy;
    public float staminaRecoveryDuration;
    public float staminaRecoveryAmount;

    [Header("Terrain Check Parameter")]
    public LayerMask groundMask;
    public float maxSlopeAngle;
    public float slopeRayLength;
    public float slopeLostDuration;

    [Header("Relax Time")]
    public float MeleeAttackInitTime;
    public float MeleeAttackRelaxTime;
    public float RangeAttackRelaxTime;

    [Header("Wall Jump Parameter")]
    public float wallJumpDuration;

    [Header("Other Parameter")]
    public float returnVelocity;
    public Vector2 knockBackForce = new Vector2(5f, 4f);
    public float hitLimit;
    public float invincibleTime;
}
