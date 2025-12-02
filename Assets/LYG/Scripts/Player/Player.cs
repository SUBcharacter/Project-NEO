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
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] CapsuleCollider2D col;
    [SerializeField] Transform muzzle;
    [SerializeField] Transform[] knifeSpawnPointUpper;
    [SerializeField] Transform[] knifeSpawnPoint;
    [SerializeField] GameObject meleeAttackHitBox;
    [SerializeField] GameObject meleeAirAttackHitBox;
    [SerializeField] GameObject[] meleeAttackHitBoxes;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] PlayerInput input;
    [SerializeField] Magazine mag;
    [SerializeField] SkillManager skillManager;
    [SerializeField] TerrainCheck check;
    [SerializeField] PlayerState currentState;
    [SerializeField] PlayerStats stats;
    [SerializeField] PlayerUI ui;
    [SerializeField] Weapon arm;
    [SerializeField] GhostTrail ghostTrail;
    [SerializeField] Coroutine fire;
    [SerializeField] Dictionary<string, PlayerState> states = new();

    [SerializeField] Vector2 groundNormal;
    [SerializeField] Vector2 currentVelocity;
    [SerializeField] WeaponState currentWeapon;
    [SerializeField] Vector2 mousePos;

    [SerializeField] float staminaTimer;
    [SerializeField] float stamina;

    [SerializeField] int health;
    [SerializeField] int meleeAttackIndex;
    [SerializeField] int bulletCount;

    [SerializeField] bool facingRight;
    [SerializeField] bool aiming;
    [SerializeField] bool dodging;
    [SerializeField] bool hit;
    [SerializeField] bool attacking;
    [SerializeField] bool isDead;

    public SpriteRenderer Ren => ren;
    public SkillManager SkMn => skillManager;
    public PlayerState CrSt => currentState;
    public PlayerStats Stats => stats;
    public PlayerUI UI => ui;
    public Dictionary<string, PlayerState> States => states;
    public GhostTrail GhTr { get => ghostTrail; set => ghostTrail = value; }
    public Weapon Arm { get => arm; set => arm = value; }
    public Rigidbody2D Rigid { get => rigid; set => rigid = value; }
    public CapsuleCollider2D Col { get => col; set => col = value; }
    public TerrainCheck Check { get => check; set => check = value; }

    public float Stamina { get => stamina; set => stamina = value; }

    public int MeleeAttackIndex { get => meleeAttackIndex; set => meleeAttackIndex = value; }
    public int BulletCount { get => bulletCount; set => bulletCount = value; }

    public bool Aiming { get => aiming; set => aiming = value; }
    public bool Dodging { get => dodging; set => dodging = value; }
    public bool _Hit { get => hit; set => hit = value; }
    public bool Attacking { get => attacking; set => attacking = value; }
    public bool IsDead => isDead;



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
        col = GetComponent<CapsuleCollider2D>();
        input = GetComponent<PlayerInput>();
        check = GetComponent<TerrainCheck>();
        ui = GetComponentInChildren<PlayerUI>();
        ren = GetComponentInChildren<SpriteRenderer>();
        arm = GetComponentInChildren<Weapon>();
        skillManager = GetComponentInChildren<SkillManager>();
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
        StaminaTimer();
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
        health = stats.maxHealth;
        bulletCount = 30;
        meleeAttackIndex = 0;
        stamina = stats.maxStamina;
        isDead = false;
        currentWeapon = WeaponState.Melee;
        ChangeState(states["Idle"]);
    }

    void StaminaTimer()
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
            Vector2 dir = (mousePos - (Vector2)arm.transform.position).normalized;

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
        if (dodging || skillManager.Charging || attacking)
            return;

        // 기본 속도
        Vector2 moveVelocity = input.MoveVec * stats.speed;

        //check.SlopeCheck();

        if (currentState is PlayerWallJumpState)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y);
            return;
        }

        if(!check.IsGround)
        {
            float newX = Mathf.Lerp(rigid.linearVelocityX, moveVelocity.x, 1f);
            rigid.linearVelocityX = newX;
            return;
        }

        // 경사면 및 일반 지형
        if (check.OnSlope && !check.Jumped)
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
        mousePos = Camera.main.ScreenToWorldPoint(input.MouseInputVec);
        ui.playerCrossHair.rectTransform.position = input.MouseInputVec;
    }

    void MeleeAttack()
    {
        if (currentState is PlayerHitState || skillManager.Charging || attacking)
            return;

        if (!(currentState is PlayerMeleeAttackState))
        {
            ChangeState(states["MeleeAttack"]);
        }

        Vector2 dir = (mousePos - (Vector2)muzzle.position).normalized;

        if(check.IsGround)
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
        if (currentState is PlayerHitState || skillManager.Charging)
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
        if (skillManager.Casting)
            return;

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        if (check.IsGround)
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
        if (skillManager.Casting)
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
        if (skillManager.Casting)
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

        Vector2 dir = (mousePos - (Vector2)arm.transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arm.transform.rotation = Quaternion.Euler(0, 0, angle);
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

    public void Jump(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || skillManager.Charging || dodging)
            return;

        if (context.started)
        {
            check.Jumped = true;
            if (check.IsGround)
            {
                rigid.linearVelocityY = 0;
                rigid.linearVelocityY = stats.jumpForce;
            }
            else if (check.CanWallJump)
            {
                Debug.Log("벽점프");

                if (check.WallLeft)
                {
                    rigid.linearVelocity = new Vector2(-stats.wallJumpX, stats.wallJumpY);
                }
                else if (check.WallRight)
                {
                    rigid.linearVelocity = new Vector2(stats.wallJumpX, stats.wallJumpY);
                }

                ChangeState(states["WallJump"]);

                check.CanWallJump = false;
            }
            else if (check.CanAirJump)
            {
                rigid.linearVelocityY = stats.airJumpForce;
                check.CanAirJump = false;
            }
        }
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || skillManager.Charging || dodging)
            return;

        if (context.performed)
        {
            // 팔 비활성화
            // 사격 상태 해제 -> 일반 상태 진입
            // 사격 코루틴 중단
            arm.EnableSprite(false);
            ChangeState(states["Idle"]);
            if (fire != null)
                StopCoroutine(fire);
            switch (currentWeapon)
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

    public void InitiateAttack(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || dodging || skillManager.Charging)
            return;

        if (context.performed)
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
        else if (context.canceled)
        {
            switch (currentWeapon)
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

    public void Dodge(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || skillManager.Charging || dodging || !check.CanDodge)
            return;
        // 회피 상태 or 회피 기회 소모시 불가
        // 회피 함수
        // 회피 기회 소모
        // 회피 상태 전환
        // 회피 속도 -> 입력 방향 * 속도

        if (context.performed)
        {
            check.CanDodge = false;
            ChangeState(states["Dodge"]);
            rigid.linearVelocityX = input.MoveVec.x * stats.dodgeForce;
        }
    }

    public void Skill1(InputAction.CallbackContext context)
    {
        Debug.Log("스킬 1");
        if (currentState is PlayerHitState)
            return;

        if (context.performed)
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

    public void Skill2(InputAction.CallbackContext context)
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

    public void Parrying(InputAction.CallbackContext context)
    {
        if (currentState is PlayerHitState || skillManager.Charging)
            return;
        // 제작 중
        if (context.performed)
        {
            Debug.Log("패링");
        }
    }

    #endregion

    

    //------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------
    // 코루틴

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
        if(check.IsGround)
        {
            attacking = false;
            meleeAirAttackHitBox.SetActive(false);
            yield break;
        }
        rigid.linearVelocityY = 0;
        rigid.AddForce(Vector2.up * 3, ForceMode2D.Impulse);
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
