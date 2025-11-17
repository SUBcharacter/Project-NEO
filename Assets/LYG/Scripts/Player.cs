using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] Transform arm;
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] Vector2 moveVec;
    [SerializeField] Vector2 mousePos;
    [SerializeField] LayerMask groundMask;

    [SerializeField] float speed;
    [SerializeField] float jumpForce;

    [SerializeField] bool isGround;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Move();
        GroundCheck();
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
        if (!isGround)
            return;

        if(context.started)
        {
            rigid.linearVelocityY = jumpForce;
        }
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
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
}
