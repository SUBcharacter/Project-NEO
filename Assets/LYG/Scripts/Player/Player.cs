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

public enum ShotMode
{
    Handgun,
    Shotgun,
    DoubleTap,
    Minigun
}

public class Player : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] CapsuleCollider2D col;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Dictionary<string, PlayerState> states = new();
    [SerializeField] GameObject meleeAttackHitBox;
    [SerializeField] GameObject meleeAirAttackHitBox;
    [SerializeField] GameObject[] meleeAttackHitBoxes;
    [SerializeField] Transform armPositon;
    [SerializeField] Transform[] knifeSpawnPointUpper;
    [SerializeField] Transform[] knifeSpawnPoint;
    [SerializeField] PlayerInput input;
    [SerializeField] SkillManager skillManager;
    [SerializeField] TerrainCheck check;
    [SerializeField] PlayerState currentState;
    [SerializeField] PlayerStats stats;
    [SerializeField] PlayerUI ui;
    [SerializeField] Weapon arm;
    [SerializeField] GhostTrail ghostTrail;
    [SerializeField] Coroutine fire;

    [SerializeField] Vector2 currentVelocity;
    [SerializeField] Vector2 mousePos;
    [SerializeField] WeaponState currentWeapon;

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
    [SerializeField] bool enhanced;

    public SpriteRenderer Ren => ren;
    public SkillManager SkMn => skillManager;
    public Transform ArmPositon => armPositon;
    public PlayerState CrSt => currentState;
    public PlayerStats Stats => stats;
    public PlayerUI UI => ui;
    public Dictionary<string, PlayerState> States => states;
    public GhostTrail GhTr { get => ghostTrail; set => ghostTrail = value; }
    public Weapon Arm { get => arm; set => arm = value; }
    public Rigidbody2D Rigid { get => rigid; set => rigid = value; }
    public CapsuleCollider2D Col { get => col; set => col = value; }
    public TerrainCheck Check { get => check; set => check = value; }


    public WeaponState CrWp => currentWeapon;

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
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        input = GetComponent<PlayerInput>();
        check = GetComponent<TerrainCheck>();
        ui = GetComponentInChildren<PlayerUI>();
        ren = GetComponentInChildren<SpriteRenderer>();
        arm = GetComponentInChildren<Weapon>();
        skillManager = GetComponentInChildren<SkillManager>();
        ghostTrail = GetComponentInChildren<GhostTrail>();
        rigid.sharedMaterial = stats.noFriction;
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
        // 현재까지 작업한 상태 등록
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
        // 기본적인 스탯 초기화
        health = stats.maxHealth;
        bulletCount = stats.maxBullet;
        meleeAttackIndex = 0;
        stamina = stats.maxStamina;
        isDead = false;
        currentWeapon = WeaponState.Melee;
        ChangeState(states["Idle"]);
    }

    void StaminaTimer()
    {
        // 스태미너 리얼 타임 회복
        // 현재 초당 10% 설정
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
            // 사격 상태 혹은 근접 공격 상태
            Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

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
            // 기본 상태
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
        // 회피, 차지어택, 근접 공격, 피격 시 리턴
        if (currentState is PlayerHitState || dodging || skillManager.Charging || attacking || arm.Firing)
            return;

        // 기본 속도 최대 속도
        float speed;
        if(arm.Mode == ShotMode.Minigun && aiming)
        {
            speed = stats.speed / 2f;
        }
        else
        {
            speed = stats.speed;
        }

        Vector2 moveVelocity = input.MoveVec * speed;

        //check.SlopeCheck();

        if (currentState is PlayerWallJumpState)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y);
            return;
        }

        if(!check.IsGround)
        {
            // 체공 상태 시 가속 이동 (가속 빠름)
            float newX = Mathf.Lerp(rigid.linearVelocityX, moveVelocity.x, 1f);
            rigid.linearVelocityX = newX;
            return;
        }

        // 경사면 및 일반 지형
        if (check.OnSlope && !check.Jumped)
        {
            // 법선 벡터의 수직 벡터 계산
            Vector2 perp = Vector2.Perpendicular(check.GroundNormal).normalized;

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
        // 차지어택, 근접공격 중, 피격 시 리턴
        if (currentState is PlayerHitState || skillManager.Charging || attacking)
            return;

        // 근접 공격 상태가 아닐 경우 진입
        if (!(currentState is PlayerMeleeAttackState))
        {
            ChangeState(states["MeleeAttack"]);
        }

        // 공격 방향 결정
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        if(check.IsGround)
        {
            // 지상 공격
            switch (meleeAttackIndex)
            {
                case 0:
                    // 1타
                    StartCoroutine(Slash(dir));
                    break;
                case 1:
                    // 2타
                    StartCoroutine(Sting(dir));
                    break;
                case 3:
                    // 3타
                    StartCoroutine(HandCannon(dir));
                    break;
            }
        }
        else
        {
            // 공중 공격
            StartCoroutine(AirSlash());
        }
    }

    void Launch()
    {
        ChangeState(states["RangeAttack"]);

        // 총알 없을 시, 차지 어택 시, 피격 시 리턴
        if (currentState is PlayerHitState || skillManager.Charging || bulletCount <= 0)
            return;

        // 사격 함수
        // 마우스 위치를 받아 방향 계산
        RotateArm();
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        arm.Launch(dir);
        bulletCount--;

        // 랜더마이징으로 탄착군 형성
        // 무기 종류에 따라 사용 가능성 있음
        //float rand = Random.Range(-3f, 3f);
        //dir = Quaternion.Euler(0, 0, rand) * dir;
        
        // 총알 풀에서 발사 및 총알 -1
        //mag.Fire(dir,muzzle.position);
        //bulletCount--;
    }

    void Death()
    {
        // TODO - 사망시 연출 및 이벤트 관리
        isDead = true;
        health = 0;
        rigid.linearVelocity = Vector2.zero;
        rigid.gravityScale = 0;
    }

    #region Skills

    // 이기어검 스킬
    void PhantomBlade()
    {
        // 스킬 사용중 시 리턴
        if (skillManager.Casting)
            return;

        // 초기 방향 결정
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        if (check.IsGround)
        {
            // 지상에 있을 경우 지형 위쪽에서만 생성
            skillManager.InitiatingPhantomBlade(knifeSpawnPointUpper, dir);
        }
        else
        {
            // 체공시 플레이어 주변 생성
            skillManager.InitiatingPhantomBlade(knifeSpawnPoint, dir);
        }
    }

    // 몸통박치기 스킬
    void ChargeAttack()
    {
        // 스킬 사용중 시 리턴
        if (skillManager.Casting)
            return;
        Debug.Log("차지어택");

        // 방향 결정 및 실행
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        skillManager.InitiatingChargeAttack(dir);
    }

    // 오토 타겟팅 스킬
    void AutoTargeting()
    {
        Debug.Log("오토 타겟팅");
        // 입력 제한은 스킬 매니저에서
        skillManager.InitiatingAutoTargeting();
    }

    // 섬광참 스킬
    void FlashAttack()
    {
        // 스킬 사용중 시 리턴
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

        // 체력 검사 후 사망 판정
        if(health <= 0)
        {
            Death();
            return;
        }
        // 피격 상태 진입
        ChangeState(states["Hit"]);
    }

    public void KnockBack()
    {
        // 플레이어의 방향에 따른 넉백
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
        // 불릿 오브 획득시 실행
        bulletCount += amount;

        if(bulletCount >= stats.maxBullet)
        {
            bulletCount = stats.maxBullet;
        }
    }

    public void GetHealth(int amount)
    {
        // 힐팩 또는 체력 회복 시 실행
        health += amount;
        if(health >= stats.maxHealth)
        {
            health = stats.maxHealth;
        }
    }

    #region Input Function
    // 입력 함수들
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
                    if(arm.Mode == ShotMode.Minigun && fire != null)
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

    #endregion



    //------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------
    // 코루틴

    // 근접 1타
    IEnumerator Slash(Vector2 dir)
    {
        // 애니메이션 완성시 변경 예정
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

    // 근접 2타
    IEnumerator Sting(Vector2 dir)
    {
        // 애니메이션 완성시 변경 예정
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

    // 근접 3타
    IEnumerator HandCannon(Vector2 dir)
    {
        // 애니메이션 완성시 변경 예정
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

    // 공중 근접 공격
    IEnumerator AirSlash()
    {
        // 애니메이션 완성시 변경 예정
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

        if (bulletCount <= 0)
            yield break;

        switch(arm.Mode)
        {
            case ShotMode.Handgun:
                Launch();
                break;
            case ShotMode.DoubleTap:
                Launch();
                break;
            case ShotMode.Shotgun:
                Launch();
                if (arm.Mode == ShotMode.Shotgun)
                    yield return CoroutineCasher.Wait(0.5f);
                break;
            case ShotMode.Minigun:
                while (true)
                {
                    Launch();
                    if (bulletCount <= 0)
                        yield break;
                    yield return CoroutineCasher.Wait(0.01f);
                }
        }

        fire = null;
    }
}
