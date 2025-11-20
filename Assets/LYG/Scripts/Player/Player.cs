using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponState
{
    Melee,Ranged
};

public class Player : MonoBehaviour
{
    [SerializeField] Transform arm;
    [SerializeField] Transform muzzle;
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Magazine mag;
    [SerializeField] Vector2 moveVec;
    [SerializeField] Vector2 mousePos;
    [SerializeField] LayerMask groundMask;
    Coroutine fire;
    public PlayerState[] states = new PlayerState[3];
    PlayerState currentState;
    WeaponState currentWeapon;

    [SerializeField] float speed;
    [SerializeField] float jumpForce;

    [SerializeField] int weaponIndex;

    [SerializeField] bool isGround;
    [SerializeField] bool canAirJump;
    [SerializeField] public bool aiming;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        ren = GetComponent<SpriteRenderer>();
        Cursor.lockState = CursorLockMode.Confined;
        arm.gameObject.SetActive(false);
        weaponIndex = 0;
        states[0] = new IdleState();
        states[1] = new AttackState();
        states[2] = new SubAttackState();
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
        Vector2 moveVelocity = moveVec * speed;
        rigid.linearVelocityX = moveVelocity.x;
    }

    void GroundCheck()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        isGround = Physics2D.CircleCast(bottomCenter, radius, Vector2.down, 0.05f, groundMask);

        if(isGround)
        {
            canAirJump = isGround;
        }
    }

    public void RotateArm()
    {
        Vector2 dir = (mousePos - (Vector2)arm.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arm.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Launch()
    {
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        if(!aiming)
        {
            float rand = Random.Range(-5f, 5f);

            dir = Quaternion.Euler(0, 0, rand) * dir;
        }

        mag.Fire(dir,muzzle.position);
    }

    public void ChangeState(PlayerState state)
    {
        currentState?.Exit(this);
        currentState = state;
        currentState?.Start(this);
    }

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
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if(isGround)
            {
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
        Vector2 inputVec = context.ReadValue<Vector2>();

        mousePos = Camera.main.ScreenToWorldPoint(inputVec);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if(aiming)
            {
                ChangeState(states[2]);
            }
            else
            {
                ChangeState(states[1]);
            }
            switch (currentWeapon)
            {
                case WeaponState.Melee:
                    break;

                case WeaponState.Ranged:
                    if (fire == null)
                    {
                        fire = StartCoroutine(AimFire());
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
            switch(currentWeapon)
            {
                case WeaponState.Melee:
                    break;
                case WeaponState.Ranged:
                    ChangeState(states[2]);
                    arm.gameObject.SetActive(aiming);
                    break;
            }
            
        }
        else if(context.canceled)
        {
            switch (currentWeapon)
            {
                case WeaponState.Melee:
                    break;
                case WeaponState.Ranged:
                    ChangeState(states[0]);
                    arm.gameObject.SetActive(aiming);
                    break;
            }
            
        }
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            arm.gameObject.SetActive(false);
            aiming = false;
            if (fire != null)
                StopCoroutine(fire);
            weaponIndex = (weaponIndex + 1) % 2;
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

    IEnumerator AimFire()
    {
        while(aiming)
        {
            Launch();
            yield return CoroutineCasher.Wait(0.1f);
        }
    }

    
}
