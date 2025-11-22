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
        /* 초기화 목록
         * 마우스 커서 상태
         * 무기 팔 상태
         * 체력
         * isDead = false
         * 무기 상태 - 근접
         * 행동 상태 - 대기
        */ 
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
        MouseConvert();
    }

    void SpriteControl()
    {
        // 마우스 위치와 입력에 따른 스프라이트 변화 - 수정 예정
        if(aiming)
        {
            // 사격 상태일시
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
            // 기본 상태 및 근접 공격 상태
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
        // 회피 중 예외 처리
        if (dodging)
            return;

        // 기본 속도
        Vector2 moveVelocity = moveVec * speed;

        // 공중 상태
        if (!isGround) 
        { 
            rigid.linearVelocity = new Vector2(moveVelocity.x, rigid.linearVelocityY); 
            return; 
        }

        // 경사면 및 일반 지형
        if (onSlope)
        {
            // 법선 벡터의 수직 벡터 계산
            Vector2 perp = Vector2.Perpendicular(groundNormal).normalized;

            // 이동 방향 조정 - 안할 시 반대로 움직이거나 방향 고정
            if (Vector2.Dot(perp, Vector2.right) < 0f)
                perp *= -1;

            // 해당 벡터에 맞게 스피드 가산 및 적용
            Vector2 slopeVel = perp * moveVelocity.x;
            rigid.linearVelocity = slopeVel;
        }
        else
        {
            // 일반 지형
            rigid.linearVelocity = new Vector2(moveVelocity.x, rigid.linearVelocityY);
        }
    }

    void MouseConvert()
    {
        // 마우스 위치 변환 및 크로스헤어 위치 트래킹
        mousePos = Camera.main.ScreenToWorldPoint(mouseInputVec);
        crosshair.GetComponent<RectTransform>().position = mouseInputVec;
    }

    void GroundCheck()
    {
        // 지형 체크

        // 캡슐 콜라이더의 하단 반원부분 중심위치 계산
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.03f; // 범위 튜닝

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        // 서클캐스트(Gizmo 옵션 적용)       
        RaycastHit2D hit = Physics2D.CircleCast(bottomCenter, radius, Vector2.down, 0.05f, groundMask);

        isGround = hit.collider != null;

        // 착지 상태 일시 초기화
        if(isGround)
        {
            canAirJump = isGround;
            canDodge = isGround;
        }
    }

    void SlopeCheck()
    {
        // 경사면 물리 연산
        // 수직, 수평을 레이캐스트로 검사 해서, 법선 벡터와, 경사각을 구해 Move함수에서 적용

        // 발사 위치는 GroundCheck의 Origin
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius + 0.2f;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        HorizontalSlopeCheck(bottomCenter);
        VerticalSlopeCheck(bottomCenter);
    }

    void VerticalSlopeCheck(Vector2 bottomCenter)
    {
        // 수직 검사
        // 레이캐스트 지속 검사로 인한 연산부담 완화
        if(!isGround || Mathf.Abs(rigid.linearVelocityY) > 0.01f)
        {
            onSlope = false;
            slopeAngle = 0f;
            groundNormal = Vector2.zero;
            return;
        }

        // 아래 방향으로 slopeRayLength(1.5f) 만큼 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(bottomCenter, Vector2.down, slopeRayLength, groundMask);

        if (hit)
        {
            // 히트시 해당 지형의 법선 벡터 저장
            // 경사각 저장
            // 경사각에 따른 onSlope 값 최신화
            // slopeLostTimer -> 해당 경사의 끝에서 튕겨져 나가는 현상 억제(해결 힘듬....)
            groundNormal = hit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            onSlope = slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
            slopeLostTimer = 0;
            Debug.DrawRay(hit.point, groundNormal, Color.green);
        }
        else
        {
            // 없을시 slopeLostTimer 작동
            // 타이머 초과시 onSlope, 경사각, 법선 벡터 초기화
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

        // 경사 유무에 따른 마찰력 계수(물리 머티리얼 2D 인자)
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
        // 수평 검사 - 좌, 우 동시 검사
        // 연산 부담 완화
        if (!isGround)
            return;

        // 좌, 우로 레이캐스트 발사
        RaycastHit2D leftHit = Physics2D.Raycast(bottomCenter, Vector2.left, slopeRayLength, groundMask);
        RaycastHit2D rightHit = Physics2D.Raycast(bottomCenter, Vector2.right, slopeRayLength, groundMask);

        // 어느쪽을 맞든 상관 없이 똑같이 최신화
        // 법선 벡터, 경사각 최신화
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
        // 회피 함수
        // 회피 기회 소모
        // 회피 상태 전환
        // 회피 속도 -> 입력 방향 * 속도
        canDodge = false;
        ChangeState(states[4]);
        rigid.linearVelocityX = moveVec.x * 30f;
    }

    void Launch()
    {
        // 사격 함수
        // 마우스 위치를 받아 방향 계산
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        // 랜더마이징으로 탄착군 형성
        float rand = Random.Range(-3f, 3f);
        dir = Quaternion.Euler(0, 0, rand) * dir;
        
        // 총알 풀에서 발사
        mag.Fire(dir,muzzle.position);
    }

    void Death()
    {
        // 제작 중
        health = 0;
        isDead = true;
    }

    void MeleeAttack()
    {
        // 제작 중
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;
    }

    #region public Function

    public void ChangeState(PlayerState state)
    {
        // 상태 전환
        currentState?.Exit(this);
        currentState = state;
        currentState?.Start(this);
    }

    public void RotateArm()
    {
        // 마우스 위치에 맞게 팔 방향 조정
        if (!aiming)
            return;

        Vector2 dir = (mousePos - (Vector2)arm.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arm.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Hit(int damage)
    {
        // 피격 함수

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
        // 회피 상태에서는 공격 불가
        if (dodging)
            return;

        // 무기 상태에 따른 분기
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
        // 제작 중
        if(context.performed)
        {
            Debug.Log("패링");
        }
        
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        // 회피 상태 or 회피 기회 소모시 불가
        if (dodging || !canDodge)
            return;
        if(context.performed)
        {
            Dodge();
        }
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        // 회피 상태시 전환 불가
        if (dodging)
            return;

        if(context.performed)
        {
            // 팔 비활성화
            // 사격 상태 해제 -> 일반 상태 진입
            // 사격 코루틴 중단
            arm.gameObject.SetActive(false);
            ChangeState(states[0]);
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
        // 레이캐스트 시각화
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottomCenter + Vector2.down * 0.05f, radius);
    }

    IEnumerator Fire()
    {
        // 사격 코루틴
        // 입력 있을 시 계속해서 사격 상태 갱신
        ChangeState(states[1]);
        Launch();
        yield return CoroutineCasher.Wait(0.1f);
        
    }

    
}
