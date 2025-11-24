using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public PhysicsMaterial2D noFriction;
    public PhysicsMaterial2D fullFriction;

    public int maxHealth;
    public int maxStamina;

    public float speed;
    public float dodgeForce;
    public float jumpForce;
    public float airJumpForce;
    public float wallJumpX;
    public float wallJumpY;
    public float maxSlopeAngle;
    public float slopeRayLength;
    public float slopeLostDuration;
    public float wallJumpDuration;
    public float relaxTime;
    public float returnVelocity;
}
