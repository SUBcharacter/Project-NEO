using System.Collections;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public enum WeaponState
{
    Melee,Ranged
};

public class Player : MonoBehaviour
{
    [SerializeField] Transform muzzle;
    [SerializeField] GameObject crosshair;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Magazine mag;
    [SerializeField] PhysicsMaterial2D fullFriction;
    [SerializeField] PhysicsMaterial2D noFriction;
    [SerializeField] public Transform arm;
    [SerializeField] public Rigidbody2D rigid;
    [SerializeField] Vector2 moveVec;
    [SerializeField] Vector2 mousePos;
    [SerializeField] Vector2 mouseInputVec;
    [SerializeField] Vector2 groundNormal;
    [SerializeField] LayerMask groundMask;
    Coroutine fire;
    public PlayerState[] states = new PlayerState[5];
    PlayerState currentState;
    WeaponState currentWeapon;

    [SerializeField] float speed;
    [SerializeField] float jumpForce;

    float slopeAngle;
    float maxSlopeAngle = 45f;
    float slopeRayLength = 1.5f;
    float slopeLostTimer;
    float slopeLostDuration = 0.1f;

    [SerializeField] int health;
    [SerializeField] int maxHealth;

    [SerializeField] bool isDead;
    [SerializeField] bool canAirJump;
    [SerializeField] public bool isGround;
    [SerializeField] public bool aiming;
    [SerializeField] public bool dodging;
    [SerializeField] bool canDodge;

    bool onSlope;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        ren = GetComponent<SpriteRenderer>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        arm.gameObject.SetActive(false);
        health = maxHealth;
        states[0] = new IdleState();
        states[1] = new RangeAttackState();
        states[2] = new MeleeAttackState();
        states[3] = new ParryingState();
        states[4] = new DodgeState();
        currentWeapon = WeaponState.Melee;
        ChangeState(states[0]);
    }

    private void Update()
    {
        currentState?.Update(this);
        SpriteControl();
        
    }

    private void FixedUpdate()
    {
        Move();
        GroundCheck();
        SlopeCheck();
        Crosshair();
    }

    void SpriteControl()
    {
        if(aiming)
        {
            Vector2 dir = (mousePos - (Vector2)arm.position).normalized;

            if(dir.x <0)
            {
                ren.flipX = true;
            }
            else if(dir.x >0)
            {
                ren.flipX = false;
            }
        }
        else
        {
            if(moveVec.x < 0)
            {
                ren.flipX = true;
            }
            else if(moveVec.x > 0)
            {
                ren.flipX = false;
            }
        }
    }

    void Move()
    {
        if (dodging)
            return;

        Vector2 moveVelocity = moveVec * speed;

        if (!isGround) 
        { 
            rigid.linearVelocity = new Vector2(moveVelocity.x, rigid.linearVelocityY); 
            return; 
        }

        if (onSlope)
        {
            Vector2 perp = Vector2.Perpendicular(groundNormal).normalized;

            if (Vector2.Dot(perp, Vector2.right) < 0f)
                perp *= -1;

            Vector2 slopeVel = perp * moveVelocity.x;
            rigid.linearVelocity = slopeVel;
        }
        else
        {
            rigid.linearVelocity = new Vector2(moveVelocity.x, rigid.linearVelocityY);
        }
    }

    void Crosshair()
    {
        mousePos = Camera.main.ScreenToWorldPoint(mouseInputVec);
        crosshair.transform.position = mousePos;
    }

    void GroundCheck()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.03f;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        RaycastHit2D hit = Physics2D.CircleCast(bottomCenter, radius, Vector2.down, 0.05f, groundMask);

        isGround = hit.collider != null;

        if(isGround)
        {
            canAirJump = isGround;
            canDodge = isGround;
        }
    }

    void SlopeCheck()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius + 0.2f;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        HorizontalSlopeCheck(bottomCenter);
        VerticalSlopeCheck(bottomCenter);
    }

    void VerticalSlopeCheck(Vector2 bottomCenter)
    {
        if(!isGround || Mathf.Abs(rigid.linearVelocityY) > 0.01f)
        {
            onSlope = false;
            slopeAngle = 0f;
            groundNormal = Vector2.zero;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(bottomCenter, Vector2.down, slopeRayLength, groundMask);

        if (hit)
        {
            groundNormal = hit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            onSlope = slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
            slopeLostTimer = 0;
            Debug.DrawRay(hit.point, groundNormal, Color.green);
        }
        else
        {
            slopeLostTimer += Time.deltaTime;
            if(slopeLostTimer < slopeLostDuration)
            {
                onSlope = true;
            }
            else
            {
                onSlope = false;
                slopeAngle = 0f;
                groundNormal = Vector2.zero;
            }
        }

        if(onSlope)
        {
            rigid.sharedMaterial = fullFriction;
        }
        else
        {
            rigid.sharedMaterial = noFriction;
        }
    }

    void HorizontalSlopeCheck(Vector2 bottomCenter)
    {
        if (!isGround)
            return;

        RaycastHit2D leftHit = Physics2D.Raycast(bottomCenter, Vector2.left, slopeRayLength, groundMask);
        RaycastHit2D rightHit = Physics2D.Raycast(bottomCenter, Vector2.right, slopeRayLength, groundMask);

        if(leftHit)
        {
            groundNormal = leftHit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            Debug.DrawRay(leftHit.point, groundNormal, Color.blue);
        }
        else if (rightHit)
        {
            groundNormal = rightHit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            Debug.DrawRay(rightHit.point, groundNormal, Color.red);
        }
        else
        {
            groundNormal = Vector2.zero;
            slopeAngle = 0f;
        }
    }

    void Dodge()
    {
        canDodge = false;
        ChangeState(states[4]);
        rigid.linearVelocityX = moveVec.x * 20f;
    }

    void Launch()
    {
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        float rand = Random.Range(-3f, 3f);

        dir = Quaternion.Euler(0, 0, rand) * dir;
        
        mag.Fire(dir,muzzle.position);
    }

    void Death()
    {
        health = 0;
        isDead = true;
    }

    void MeleeAttack()
    {
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;
    }

    #region public Function

    public void ChangeState(PlayerState state)
    {
        currentState?.Exit(this);
        currentState = state;
        currentState?.Start(this);
    }

    public void RotateArm()
    {
        if (!aiming)
            return;

        Vector2 dir = (mousePos - (Vector2)arm.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arm.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Hit(int damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Death();
        }
    }

    #endregion

    #region Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            moveVec = context.ReadValue<Vector2>();
        }
        else if(context.canceled)
        {
            moveVec = Vector2.zero;
            rigid.linearVelocity = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (dodging)
            return;

        if(context.started)
        {
            if(isGround)
            {
                rigid.linearVelocityY = 0;
                rigid.linearVelocityY = jumpForce;
            }
            else if(canAirJump)
            {
                rigid.linearVelocityY = jumpForce;
                canAirJump = false;
            }
            
        }
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
       mouseInputVec = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (dodging)
            return;

        if(context.performed)
        {
            switch (currentWeapon)
            {
                case WeaponState.Melee:
                    break;

                case WeaponState.Ranged:
                    if (fire == null)
                    {
                        fire = StartCoroutine(Fire());
                    }
                    break;
            }

        }
        else if(context.canceled)
        {
            switch(currentWeapon)
            {
                case WeaponState.Melee:
                    break;
                case WeaponState.Ranged:
                    if (fire != null)
                    {
                        StopCoroutine(fire);
                        fire = null;
                    }
                    break;
            }
        }
    }

    public void OnSubAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Debug.Log("ÆÐ¸µ");
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (dodging || !canDodge)
            return;
        if(context.performed)
        {
            Dodge();
        }
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        if (dodging)
            return;

        if(context.performed)
        {
            arm.gameObject.SetActive(false);
            aiming = false;
            if (fire != null)
                StopCoroutine(fire);
            switch(currentWeapon)
            {
                case WeaponState.Melee:
                    currentWeapon = WeaponState.Ranged;
                    break;
                case WeaponState.Ranged:
                    currentWeapon = WeaponState.Melee;
                    break;
            }
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottomCenter + Vector2.down * 0.05f, radius);
    }

    IEnumerator Fire()
    {
        while(true)
        {
            ChangeState(states[1]);
            Launch();
            yield return CoroutineCasher.Wait(0.1f);
        }
    }

    
}
