using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static UnityEditor.VersionControl.Asset;

public enum ResearcherStateType
{
    Idle, Walk, Chase, Summon, Attack, Hit, Dead
}

// hitFlash 머티리얼을 인자로 받기
// bool hitted 선언
// HitFlash코루틴은 비실이, 퉁퉁이, EnhancableMelee 스크립트 참고 할 것.

public class Researcher : Enemy
{
    [SerializeField] public Transform[] dronespawnpoints;
    [SerializeField] public Transform Player_Trans;
    [SerializeField] public Transform arm;
    [SerializeField] public Transform Gunfire;

    [SerializeField] public GameObject Bullet_prefab;
    [SerializeField] public GameObject D_prefab;

    [SerializeField] Dictionary<ResearcherStateType, ResearcherState> R_States = new();

    [SerializeField] public Animator animator;
    [SerializeField] private Animator Armanima;

    [SerializeField] Material hitFlash;
    [SerializeField] bool hitted;
    public ResearcherState currentStates;

    public SightRange sightRange;
    public AimRange aimRange;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private float knockBackXForce = 0.5f;
    private float wallCheckDistance = 1f;
    private float groundCheckDistance = 1f;
    private float M_direction;
    public float nextFireTime;
    public float AimRotationSpeed = 5f;
    private Coroutine flashCoroutine;

    public bool isDroneSummoned;
    public bool isarmlock;
    public Dictionary<ResearcherStateType, ResearcherState> r_states => R_States;

    public ResearcherState previousState;
    protected override void Awake()
    {

        startPos = transform.position;
        sightRange = GetComponent<SightRange>();   
        aimRange = GetComponent<AimRange>();
        animator = GetComponentInChildren<Animator>();
        Ren = GetComponentInChildren<SpriteRenderer>();
        Rigid = GetComponent<Rigidbody2D>();
        hitted = false;
        R_State();
        InitArm();

    }
    private void OnEnable()
    {
        Init();
    }
    public override void Init()
    {
        currnetHealth = Stat.MaxHp;
        isDroneSummoned = false;
        isarmlock = false;
        ChangeState(r_states[ResearcherStateType.Idle]);
    }

    void InitArm()
    {
        arm = transform.Find("Armposition");

        if (arm != null)
        {
            Transform arm_trans = arm.Find("ArmSprite");
       
            Armanima = arm_trans.GetComponent<Animator>();
   
        }

        Gunfire = transform.Find("GunTip");

        arm.gameObject.SetActive(false);
    }
    void R_State()
    {
        r_states[ResearcherStateType.Idle] = new R_IdleState();
        r_states[ResearcherStateType.Walk] = new R_WalkState();
        r_states[ResearcherStateType.Summon] = new R_SummonDroneState();
        r_states[ResearcherStateType.Attack] = new R_Attackstate();
        r_states[ResearcherStateType.Hit] = new R_Hitstate();
        r_states[ResearcherStateType.Chase] = new R_ChaseState();
        r_states[ResearcherStateType.Dead] = new R_Deadstate();
    }
    void Update()
    {
        currentStates?.Update(this);
    }

    public void ChangeState(ResearcherState newState)
    {
        if (currentStates == newState) return;

        previousState = currentStates;

        currentStates?.Exit(this);
        currentStates = newState;
        currentStates?.Start(this); 
    }
    
    public void Move()
    {
        M_direction = Mathf.Sign(transform.localScale.x);
        float velocityX = M_direction * Stat.moveSpeed;
        Rigid.linearVelocity = new Vector2(velocityX, Rigid.linearVelocity.y);
    }

    public  void Chase()
    {
        float directionToPlayer = Player_Trans.position.x - transform.position.x;

        float M_direction = Mathf.Sign(directionToPlayer);

        float velocityX = M_direction * Stat.moveSpeed;

        Rigid.linearVelocity = new Vector2(velocityX, Rigid.linearVelocity.y);

        FlipResearcher(this, directionToPlayer);
        FlipArm(directionToPlayer); 

    }

    protected override void Die()
    {
        Rigid.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
        Debug.Log("Researcher 사망");
    }

    #region 애니메이션 키 이벤트 함수
    public void R_die()
    {
      Die();
    }
    
    public void summontoattack()
    {
        ChangeState(R_States[ResearcherStateType.Attack]);
    }
    #endregion

    #region 벽과 낭떠러지 체크
    public bool CheckForObstacle()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 checkDirection = (direction > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, wallCheckDistance, wallLayer);

        return hit.collider != null;
    }

    public bool CheckForLedge( )
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector3 footPosition = transform.position;
        footPosition.x += direction * 0.3f;

        RaycastHit2D hit = Physics2D.Raycast(footPosition, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider == null;
    }
    #endregion

    #region 팔 관련 함수
    public void Armsetactive(bool isactive)
    {
        arm.gameObject.SetActive(isactive);

        if (isactive)
        {
            arm.rotation = Quaternion.Euler(0, 0, 0);

            float currentDirection = transform.localScale.x;

            Vector3 armLocalScale = arm.localScale;

            if (currentDirection < 0) 
            {
                armLocalScale.x = -1;
                armLocalScale.y = -1;
            }
            else 
            {
                armLocalScale.x = 1;
                armLocalScale.y = 1;
            }

            arm.localScale = armLocalScale;

            if (!isarmlock)
            {
                Aimatplayer(true); 
            }

        }
    }
    public void Aimatplayer(bool active = false)
    {
        if (isarmlock) return;

        Vector3 dir = Player_Trans.position - arm.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        if (active) 
        {
            arm.rotation = targetRotation;
        }
        else 
        {
            arm.rotation = Quaternion.Slerp(arm.rotation,targetRotation,Time.deltaTime * AimRotationSpeed);
        }
    }

    public void FlipArm(float direction)
    {

        Vector3 armLocalScale = arm.localScale;

        if (direction < 0) // 왼쪽을 바라볼 때
        {

            armLocalScale.x = -1;
            armLocalScale.y = -1;

        }
        else // 오른쪽을 바라볼 때
        {

            armLocalScale.x = 1;
            armLocalScale.y = 1;

        }

        arm.localScale = armLocalScale;
    }
    public void FlipToTargetX(float targetX)
    {
        if (isarmlock) return;

        float dir = targetX - transform.position.x;
        if (Mathf.Abs(dir) < 0.01f) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(dir) * Mathf.Abs(scale.x);
        transform.localScale = scale;
        FlipArm(dir);
    }
    public void ShootBullet()
    {
        Vector3 startPosition = Gunfire != null ? Gunfire.position : arm.position;
        Vector2 dirToTarget = arm.right;

        GameObject bulletObject = Instantiate(Bullet_prefab, startPosition, Quaternion.identity);
        R_Bullet bulletComponent = bulletObject.GetComponent<R_Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.Init(dirToTarget, startPosition);
        }
        Debug.Log("발사");
    }
    public override void Attack()
    {

        if (aimRange != null && aimRange.IsPlayerInSight)
        {
            Aimatplayer();
            if (Time.time >= nextFireTime)
            {

                PlayShot();
                nextFireTime = Time.time + Stat.fireCooldown;
            }
        }
        else
        {
            Debug.Log("사격 범위 이탈, 시야 유지. 추적으로 전환.");
            ChangeState(r_states[ResearcherStateType.Chase]);
        }
    }

    public void PlayShot()
    {
        isarmlock = true;
        Armanima.Play("R_Shot");
    }

    public void Armshotend()
    {
        isarmlock = false;
    }
    #endregion

    #region 방향 전환
    public void WallorLedgeFlip(Researcher researcher)
    {
        if (CheckForObstacle() || CheckForLedge())
        {
            Stat.moveDistance *= -1;
            researcher.FlipResearcher(researcher, Stat.moveDistance);
            return;
        }
       
        Move();
        
    }
    public void FlipResearcher(Researcher researcher, float direction)
    {
        if (isarmlock) return;

        Vector3 currentScale = researcher.transform.localScale;

        if (direction > 0 && currentScale.x < 0)
        {
            researcher.transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

        else if (direction < 0 && currentScale.x > 0)
        {
            researcher.transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

#endregion
    public void SummonDrone()
    {
        int rand = Random.Range(0, dronespawnpoints.Length);
        Transform spawnPos = dronespawnpoints[rand];
        isDroneSummoned = true;
        GameObject droneObject = Instantiate(D_prefab, spawnPos.position, Quaternion.identity);
        SummonDrone droneComponent = droneObject.GetComponent<SummonDrone>();
        if (droneComponent != null)
        {
            droneComponent.SummonInit(this.transform, Player_Trans);
        }
    }
       
    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;
        Debug.Log("체력 : " + currnetHealth);

        StartCoroutine(HitFlash());

        if (currnetHealth <= 0)
        {
            ChangeState(r_states[ResearcherStateType.Dead]);
            return;
        }
      
        ChangeState(r_states[ResearcherStateType.Hit]);
   
    }

    public void Knockback()
    {
        Vector2 knockbackDirection = (Vector2)transform.position - (Vector2)Player_Trans.position;
        float xDirection = Mathf.Sign(knockbackDirection.x);
        Vector2 knockbackForce = new Vector2(xDirection * knockBackXForce, 0f);
        rigid.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    IEnumerator HitFlash()
    {
        if (hitted == false)
        {
            hitted = true;
            Material origin = ren.material;
            ren.material = hitFlash;
            yield return CoroutineCasher.Wait(0.1f);
            ren.material = origin;
            hitted = false;
        }
    }
}
