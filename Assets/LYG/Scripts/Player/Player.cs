using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Transform[] knifeSpawnPointUpper;
    [SerializeField] Transform[] knifeSpawnPoint;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] GameObject meleeAttackHitBox;
    [SerializeField] GameObject meleeAirAttackHitBox;
    [SerializeField] GameObject[] meleeAttackHitBoxes;
    [SerializeField] Magazine mag;
    [SerializeField] SkillManager skillManager;
    public GhostTrail ghostTrail;
    public Rigidbody2D rigid;
    public Transform arm;
    public CapsuleCollider2D col;
    public PlayerUI UI;

    [SerializeField] Vector2 moveVec;
    [SerializeField] Vector2 mouseInputVec;
    [SerializeField] Vector2 groundNormal;
    [SerializeField] Vector2 currentVelocity;
    [SerializeField] LayerMask groundMask;
    [SerializeField] public Vector2 mousePos;
    [SerializeField] WeaponState currentWeapon;

    public PlayerStats stats;

    [SerializeField] int health;
    public int meleeAttackIndex;
    public int bulletCount;


    [SerializeField] bool facingRight;
    [SerializeField] bool canAirJump;
    [SerializeField] bool canDodge;
    [SerializeField] bool wallLeft;
    [SerializeField] bool wallRight;
    [SerializeField] bool onSlope;
    [SerializeField] bool jumped;
    public bool isDead;
    public bool isGround;
    public bool attacking;
    public bool aiming;
    public bool dodging;
    public bool charging;
    public bool onWall;
    public bool canWallJump;
    public bool hit;

    Coroutine fire;
    PlayerState currentState;
    public Dictionary<string,PlayerState> states = new();

    float slopeAngle;
    float slopeLostTimer;

    float staminaTimer;
    public float stamina;


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
        col = GetComponent<CapsuleCollider2D>();
        skillManager = GetComponentInChildren<SkillManager>();
        UI = GetComponentInChildren<PlayerUI>();
        ghostTrail = GetComponentInChildren<GhostTrail>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        StateInit();
        PlayerStatInit();
    }

    private void Update()
    {
        if (isDead)
            return;
        
        currentState?.Update(this);
        Stamina();
        if (hit)
            return;
        SpriteControl();
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;
        MouseConvert();
        if (hit)
            return;
        Move();
        GroundCheck();
        WallCheck();
    }
    void StateInit()
    {
        states["Idle"] = new PlayerIdleState();
        states["RangeAttack"] = new PlayerRangeAttackState();
        states["MeleeAttack"] = new PlayerMeleeAttackState();
        states["Parrying"] = new PlayerParryingState();
        states["Dodge"] = new PlayerDodgeState();
        states["Climb"] = new PlayerClimbState();
        states["WallJump"] = new PlayerWallJumpState();
        states["Hit"] = new PlayerHitState();
    }

    void PlayerStatInit()
    {
        arm.gameObject.SetActive(false);
        health = stats.maxHealth;
        bulletCount = 30;
        meleeAttackIndex = 0;
        stamina = stats.maxStamina;
        isDead = false;
        currentWeapon = WeaponState.Melee;
        ChangeState(states["Idle"]);
    }

    void Stamina()
    {
        if (isDead)
            return;
        staminaTimer += Time.deltaTime;

        if(staminaTimer > stats.staminaRecoveryDuration)
        {
            staminaTimer = 0;
            stamina += stats.staminaRecoveryAmount;
            if(stamina >= stats.maxStamina)
            {
                stamina = stats.maxStamina;
            }
        }

    }

    void SpriteControl()
    {
        if (currentState is PlayerHitState)
            return;
        // 마우스 위치와 입력에 따른 스프라이트 변화 - 수정 예정
        if(aiming || attacking)
        {
            // 사격 상태일시
            Vector2 dir = (mousePos - (Vector2)arm.position).normalized;

            if(dir.x <0)
            {
                ren.flipX = true;
                arm.transform.localScale = new Vector3(1, -1, 1);
                meleeAttackHitBox.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if(dir.x >0)
            {
                ren.flipX = false;
                arm.transform.localScale = new Vector3(1, 1, 1);
                meleeAttackHitBox.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            // 기본 상태 및 근접 공격 상태
            if(rigid.linearVelocityX < -0.0001f)
            {
                ren.flipX = true;
                arm.transform.localScale = new Vector3(1, -1, 0);
            }
            else if(rigid.linearVelocityX > 0.0001f)
            {
                ren.flipX = false;
                arm.transform.localScale = new Vector3(1, 1, 1);

            }
        }
            facingRight = !ren.flipX;
    }

    void Move()
    {
        if (currentState is PlayerHitState)
            return;
        // 회피 중 예외 처리
        if (dodging || charging || attacking)
            return;

        // 기본 속도
        Vector2 moveVelocity = moveVec * stats.speed;

        //SlopeCheck();

        if (currentState is PlayerWallJumpState)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y);
            return;
        }

        if(!isGround)
        {
            float newX = Mathf.Lerp(rigid.linearVelocityX, moveVelocity.x, 1f);
            rigid.linearVelocityX = newX;
            return;
        }

        // 경사면 및 일반 지형
        if (onSlope && !jumped)
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
            float newX = Mathf.Lerp(rigid.linearVelocityX, moveVelocity.x, 1f);
            rigid.linearVelocityX = newX;
        }
    }

    void MouseConvert()
    {
        // 마우스 위치 변환 및 크로스헤어 위치 트래킹
        mousePos = Camera.main.ScreenToWorldPoint(mouseInputVec);
        UI.playerCrossHair.rectTransform.position = mouseInputVec;
    }

    void GroundCheck()
    {
        if (currentState is PlayerHitState)
            return;
        // 지형 체크

        // 캡슐 콜라이더의 하단 반원부분 중심위치 계산
        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.03f; // 범위 튜닝

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        // 서클캐스트(Gizmo 옵션 적용)       
        RaycastHit2D hit = Physics2D.CircleCast(bottomCenter, radius, Vector2.down, 0.05f, groundMask);

        if(hit.collider != null)
        {
            Vector2 nor = hit.normal;

            bool isVerticalWall = Mathf.Abs(nor.x) >= 1f && Mathf.Abs(nor.y) <= 0f;

            if(isVerticalWall)
            {
                isGround = false;
            }
            else
            {
                isGround = true;
            }
        }
        else
        {
            isGround = false;
        }

        // 착지 상태 일시 초기화
        if(isGround)
        {
            canAirJump = isGround;
            canDodge = isGround;
            jumped = !isGround;
            onWall = false;
        }
    }

    void SlopeCheck()
    {
        if (currentState is PlayerHitState || charging)
            return;
        // 경사면 물리 연산
        // 수직, 수평을 레이캐스트로 검사 해서, 법선 벡터와, 경사각을 구해 Move함수에서 적용

        // 발사 위치는 GroundCheck의 Origin

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.2f;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        HorizontalSlopeCheck(bottomCenter);
        if (!onSlope)
            return;
        VerticalSlopeCheck(bottomCenter);
    }

    void VerticalSlopeCheck(Vector2 bottomCenter)
    {
        // 수직 검사
        // 레이캐스트 지속 검사로 인한 연산부담 완화
        if(!isGround)
        {
            onSlope = false;
            slopeAngle = 0f;
            groundNormal = Vector2.zero;
            return;
        }

        // 아래 방향으로 slopeRayLength(1.5f) 만큼 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(bottomCenter, Vector2.down, stats.slopeRayLength, groundMask);
        Debug.DrawRay(bottomCenter, Vector2.down * stats.slopeRayLength, Color.green);
        if (hit)
        {
            // 히트시 해당 지형의 법선 벡터 저장
            // 경사각 저장
            // 경사각에 따른 onSlope 값 최신화
            // slopeLostTimer -> 해당 경사의 끝에서 튕겨져 나가는 현상 억제(해결 힘듬....)

            groundNormal = hit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            onSlope = slopeAngle > 0f && slopeAngle <= stats.maxSlopeAngle;
            slopeLostTimer = 0;
            Debug.DrawRay(hit.point, groundNormal, Color.green);
        }
        else
        {
            // 없을시 slopeLostTimer 작동
            // 타이머 초과시 onSlope, 경사각, 법선 벡터 초기화
            slopeLostTimer += Time.deltaTime;
            if(slopeLostTimer < stats.slopeLostDuration)
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
            rigid.sharedMaterial = stats.fullFriction;
        }
        else
        {
            rigid.sharedMaterial = stats.noFriction;
        }
    }

    void HorizontalSlopeCheck(Vector2 bottomCenter)
    {
        // 수평 검사 - 좌, 우 동시 검사
        // 연산 부담 완화
        if (!isGround)
            return;

        // 좌, 우로 레이캐스트 발사
        RaycastHit2D leftHit = Physics2D.Raycast(bottomCenter, Vector2.left, stats.slopeRayLength, groundMask);
        RaycastHit2D rightHit = Physics2D.Raycast(bottomCenter, Vector2.right, stats.slopeRayLength, groundMask);

        Debug.DrawRay(bottomCenter, Vector2.left * stats.slopeRayLength, Color.magenta);
        Debug.DrawRay(bottomCenter, Vector2.right * stats.slopeRayLength, Color.green);
        // 어느쪽을 맞든 상관 없이 똑같이 최신화
        // 법선 벡터, 경사각 최신화
        if (leftHit)
        {
            groundNormal = leftHit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            onSlope = true;
            Debug.DrawRay(leftHit.point, groundNormal, Color.blue);
        }
        else if (rightHit)
        {
            groundNormal = rightHit.normal;
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
            onSlope = true;
            Debug.DrawRay(rightHit.point, groundNormal, Color.red);
        }
        else
        {
            groundNormal = Vector2.zero;
            slopeAngle = 0f;
            onSlope = false;
        }

        if (onSlope)
        {
            rigid.sharedMaterial = stats.fullFriction;
        }
        else
        {
            rigid.sharedMaterial = stats.noFriction;
        }
    }

    void WallCheck()
    {
        if (currentState is PlayerHitState)
            return;
        if ((currentState is PlayerWallJumpState) || isGround || dodging || charging)
            return;

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.2f;

        Vector2 origin = (Vector2)transform.position + new Vector2(0, bottomY);
        float rayDistance = (col.size.x / 2f) + 0.02f;

        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, rayDistance, groundMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, rayDistance, groundMask);

        Debug.DrawRay(origin, Vector2.right * rayDistance, Color.red);
        Debug.DrawRay(origin, Vector2.left * rayDistance, Color.blue);

        if(hitRight)
        {
            wallLeft = true;
            if(!(currentState is PlayerClimbState))
            {
                canWallJump = true;
                ChangeState(states["Climb"]);
            }
        }
        else if(hitLeft)
        {
            wallRight = true;
            if (!(currentState is PlayerClimbState))
            {
                canWallJump = true;
                ChangeState(states["Climb"]);
            }
        }
        else
        {
            wallLeft = false;
            wallRight = false;
            if (aiming || currentState is PlayerMeleeAttackState)
                return;
            if(!(currentState is PlayerIdleState))
            {
                ChangeState(states["Idle"]);
            }
        }
    }

    void Dodge()
    {
        if (currentState is PlayerHitState || charging)
            return;
        // 회피 함수
        // 회피 기회 소모
        // 회피 상태 전환
        // 회피 속도 -> 입력 방향 * 속도
        canDodge = false;
        ChangeState(states["Dodge"]);
        rigid.linearVelocityX = moveVec.x * stats.dodgeForce;
    }

    void MeleeAttack()
    {
        if (currentState is PlayerHitState || charging || attacking)
            return;

        if (!(currentState is PlayerMeleeAttackState))
        {
            ChangeState(states["MeleeAttack"]);
        }

        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        if(isGround)
        {
            switch (meleeAttackIndex)
            {
                case 0:
                    StartCoroutine(Slash(dir));
                    break;
                case 1:
                    StartCoroutine(Sting(dir));
                    break;
                case 3:
                    StartCoroutine(HandCannon(dir));
                    break;
            }
        }
        else
        {
            StartCoroutine(AirSlash());
        }
    }

    void Launch()
    {
        if (currentState is PlayerHitState || charging)
            return;
        if (bulletCount <= 0)
            return;
        // 사격 함수
        // 마우스 위치를 받아 방향 계산
        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        // 랜더마이징으로 탄착군 형성
        //float rand = Random.Range(-3f, 3f);
        //dir = Quaternion.Euler(0, 0, rand) * dir;
        
        // 총알 풀에서 발사
        mag.Fire(dir,muzzle.position);
        bulletCount--;
    }

    void Death()
    {
        // 제작 중
        isDead = true;
        health = 0;
        rigid.linearVelocity = Vector2.zero;
        rigid.gravityScale = 0;
    }

    #region Skills

    void PhantomBlade()
    {
        if (skillManager.casting)
            return;

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        if (isGround)
        {
            skillManager.InitiatingPhantomBlade(knifeSpawnPointUpper, dir);
        }
        else
        {
            skillManager.InitiatingPhantomBlade(knifeSpawnPoint, dir);
        }
    }

    void ChargeAttack()
    {
        if (skillManager.casting)
            return;
        Debug.Log("차지어택");

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        skillManager.InitiatingChargeAttack(dir);
    }

    void AutoTargeting()
    {
        Debug.Log("오토 타겟팅");

        skillManager.InitiatingAutoTargeting();
    }

    void FlashAttack()
    {
        if (skillManager.casting)
            return;
        Debug.Log("섬광참");
        skillManager.InitiatingFlashAttack(facingRight);
    }

    #endregion

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
            return;
        }
        ChangeState(states["Hit"]);
    }

    public void KnockBack()
    {
        if (facingRight)
        {
            rigid.linearVelocity = new Vector2(-stats.knockBackForce.x, stats.knockBackForce.y);
        }
        else
        {
            rigid.linearVelocity = stats.knockBackForce;
        }
    }

    public void GetBullet(int amount)
    {
        bulletCount += amount;

        if(bulletCount >= stats.maxBullet)
        {
            bulletCount = stats.maxBullet;
        }
    }

    public void GetHealth(int amount)
    {
        health += amount;
        if(health >= stats.maxHealth)
        {
            health = stats.maxHealth;
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
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || charging)
            return;
        if (dodging)
            return;

        if(context.started)
        {
            jumped = true;
            if(isGround)
            {
                rigid.linearVelocityY = 0;
                rigid.linearVelocityY = stats.jumpForce;
            }
            else if(canWallJump)
            {
                Debug.Log("벽점프");
                
                if(wallLeft)
                {
                    rigid.linearVelocity = new Vector2(-stats.wallJumpX, stats.wallJumpY);
                }
                else if(wallRight)
                {
                    rigid.linearVelocity = new Vector2(stats.wallJumpX, stats.wallJumpY);
                }

                ChangeState(states["WallJump"]);

                canWallJump = false;
            }
            else if(canAirJump)
            {
                rigid.linearVelocityY = stats.airJumpForce;
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
        if (currentState is PlayerHitState)
            return;
        // 회피 상태에서는 공격 불가
        if (dodging || charging)
            return;

        // 무기 상태에 따른 분기
        if(context.performed)
        {
            switch (currentWeapon)
            {
                case WeaponState.Melee:
                    MeleeAttack();
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
        if (currentState is PlayerHitState || charging)
            return;
        // 제작 중
        if (context.performed)
        {
            Debug.Log("패링");
        }
        
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || charging)
            return;
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
        if (currentState is PlayerHitState || charging)
            return;
        // 회피 상태시 전환 불가
        if (dodging)
            return;

        if(context.performed)
        {
            // 팔 비활성화
            // 사격 상태 해제 -> 일반 상태 진입
            // 사격 코루틴 중단
            arm.gameObject.SetActive(false);
            ChangeState(states["Idle"]);
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

    public void OnSkill1(InputAction.CallbackContext context)
    {
        Debug.Log("스킬 1");
        if (currentState is PlayerHitState)
            return;

        if(context.performed)
        {
            switch (currentWeapon)
            {
                case WeaponState.Melee:
                    ChargeAttack();
                    break;
                case WeaponState.Ranged:
                    PhantomBlade();
                    break;
            }
        }
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        Debug.Log("스킬 2");
        if (currentState is PlayerHitState)
            return;
        
        if (context.performed)
        {
            switch (currentWeapon)
            {
                case WeaponState.Melee:
                    FlashAttack();
                    break;
                case WeaponState.Ranged:
                    AutoTargeting();
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

    //IEnumerator Slash(Vector2 dir)
    //{
    //    if(!(currentState is PlayerMeleeAttackState))
    //    {
    //        ChangeState(states["MeleeAttack"]);
    //    }
    //    attacking = true;
    //    if(isGround)
    //    {
    //        meleeAttackHitBoxes[meleeAttackIndex].SetActive(true);
    //        rigid.linearVelocity = Vector2.zero;
    //        if (dir.x > 0)
    //        {
    //            rigid.linearVelocityX = meleeAttackIndex != 2 ? 3 : 6;
    //        }
    //        else
    //        {
    //            rigid.linearVelocityX = meleeAttackIndex != 2 ? -3 : -6;
    //        }
    //    }
    //    else
    //    {
    //        meleeAirAttackHitBox.SetActive(true);
    //    }
    //
    //    yield return CoroutineCasher.Wait(0.1f);
    //
    //    if(isGround)
    //    {
    //        rigid.linearVelocity = Vector2.zero;
    //        meleeAttackHitBoxes[meleeAttackIndex].SetActive(false);
    //        meleeAttackIndex = (meleeAttackIndex + 1) % meleeAttackHitBoxes.Length;
    //        meleeAirAttackHitBox.SetActive(false);
    //    }
    //    else
    //    {
    //        meleeAirAttackHitBox.SetActive(false);
    //    }
    //    
    //
    //    yield return CoroutineCasher.Wait(0.1f);
    //    attacking = false;
    //    
    //}

    IEnumerator Slash(Vector2 dir)
    {
        Debug.Log("베기");
        attacking = true;

        meleeAttackHitBoxes[meleeAttackIndex].SetActive(true);

        rigid.linearVelocity = Vector2.zero;
        if(dir.x > 0)
        {
            rigid.linearVelocityX = 4;
        }
        else
        {
            rigid.linearVelocityX = -4;
        }

        yield return CoroutineCasher.Wait(0.1f);

        rigid.linearVelocity = Vector2.zero;
        meleeAttackHitBoxes[meleeAttackIndex].SetActive(false);
        meleeAttackIndex = (meleeAttackIndex + 1) % meleeAttackHitBoxes.Length;

        yield return CoroutineCasher.Wait(0.1f);

        attacking = false;
    }

    IEnumerator Sting(Vector2 dir)
    {
        Debug.Log("베기");
        attacking = true;

        meleeAttackHitBoxes[meleeAttackIndex].SetActive(true);

        yield return CoroutineCasher.Wait(0.05f);

        meleeAttackHitBoxes[meleeAttackIndex].SetActive(false);
        meleeAttackIndex = (meleeAttackIndex + 1) % meleeAttackHitBoxes.Length;

        meleeAttackHitBoxes[meleeAttackIndex].SetActive(true);

        rigid.linearVelocity = Vector2.zero;
        if (dir.x > 0)
        {
            rigid.linearVelocityX = 7;
        }
        else
        {
            rigid.linearVelocityX = -7;
        }

        yield return CoroutineCasher.Wait(0.05f);

        rigid.linearVelocity = Vector2.zero;
        meleeAttackHitBoxes[meleeAttackIndex].SetActive(false);
        meleeAttackIndex = (meleeAttackIndex + 1) % meleeAttackHitBoxes.Length;

        yield return CoroutineCasher.Wait(0.1f);

        attacking = false;
    }

    IEnumerator HandCannon(Vector2 dir)
    {
        Debug.Log("근접 사격");
        attacking = true;

        meleeAttackHitBoxes[meleeAttackIndex].SetActive(true);
        rigid.linearVelocity = Vector2.zero;

        if (dir.x > 0)
        {
            rigid.linearVelocityX = -3;
        }
        else
        {
            rigid.linearVelocityX = 3;
        }
        yield return CoroutineCasher.Wait(0.1f);

        
        meleeAttackHitBoxes[meleeAttackIndex].SetActive(false);
        meleeAttackIndex = (meleeAttackIndex + 1) % meleeAttackHitBoxes.Length;

        yield return CoroutineCasher.Wait(0.1f);

        rigid.linearVelocity = Vector2.zero;

        attacking = false;

    }

    IEnumerator AirSlash()
    {
        attacking = true;

        meleeAirAttackHitBox.SetActive(true);
        Debug.Log("에어 슬래쉬!");
        if(isGround)
        {
            attacking = false;
            meleeAirAttackHitBox.SetActive(false);
            yield break;
        }
        rigid.linearVelocityY = 0;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        yield return CoroutineCasher.Wait(0.1f);

        meleeAirAttackHitBox.SetActive(false);

        yield return CoroutineCasher.Wait(0.1f);
        attacking = false;
    }

    IEnumerator Fire()
    {
        // 사격 코루틴
        // 입력 있을 시 계속해서 사격 상태 갱신
        ChangeState(states["RangeAttack"]);
        Launch();
        yield return null;
    }
}
