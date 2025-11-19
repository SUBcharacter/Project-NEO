using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] float speed;
    [SerializeField] float jumpForce;

    [SerializeField] bool isGround;
    [SerializeField] bool canAirJump;
    [SerializeField] bool aiming;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        ren = GetComponent<SpriteRenderer>();
        Cursor.lockState = CursorLockMode.Confined;
        arm.gameObject.SetActive(false);
    }

    private void Update()
    {
        RotateArm();
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

    void RotateArm()
    {
        if (!aiming)
            return;

        Vector2 dir = (mousePos - (Vector2)arm.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arm.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Launch()
    {
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        mag.Fire(dir,muzzle.position);
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
            Launch();
        }
        else if(context.canceled)
        {

        }
        
    }

    public void OnSubAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            aiming = true;
            arm.gameObject.SetActive(aiming);
        }
        else if(context.canceled)
        {
            aiming = false;
            arm.gameObject.SetActive(aiming);
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
            yield return CoroutineCasher.Wait()
        }
    }
}
