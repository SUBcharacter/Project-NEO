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


public class Researcher : Enemy
{
    [SerializeField] public Transform[] dronespawnpoints;
     public Transform[] D_SpawnPoints => dronespawnpoints;
    [SerializeField] private Transform playerTransform;
    public Transform Player_Trans => playerTransform;

    [SerializeField] public Transform arm { get; private set; }
    [SerializeField] public Transform Gunfire { get; private set; }

    [SerializeField] public GameObject D_prefab;
    [SerializeField] Magazine bulletpool;
    [SerializeField] Material hitFlash;

    [SerializeField] Dictionary<ResearcherStateType, ResearcherState> R_States = new();
    public Dictionary<ResearcherStateType, ResearcherState> r_states => R_States;

    [SerializeField] public Animator animator { get; private set; }
    [SerializeField] public Animator Armanima { get; private set; }

    public ResearcherState currentStates { get; private set; }
    public ResearcherState previousState { get; private set; }

    public SightRange sightRange { get; private set; }
    public AimRange aimRange { get; private set; }

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private float knockBackXForce = 0.5f;
    private float wallCheckDistance = 1f;
    private float groundCheckDistance = 1f;
    private float M_direction;
    public float AimRotationSpeed = 5f;

    [SerializeField] bool hitted;
    public bool isDroneSummoned { get; private set; }
    public bool isarmlock { get; private set; }
    public bool isbodylock { get; private set; }
    public float nextFireTime { get; private set; }


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
        isbodylock = false;
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

    public void Chase()
    {
        float targerdirection = Player_Trans.position.x - transform.position.x;
        float directionSign = Mathf.Sign(targerdirection);


        float distance = Mathf.Abs(targerdirection); 

        if (distance > 1.0f)
        {
            FlipResearcher(directionSign);
            Rigid.linearVelocity = new Vector2(directionSign * Stat.moveSpeed, Rigid.linearVelocity.y);
        }
        else
        {
            Rigid.linearVelocity = new Vector2(0, Rigid.linearVelocity.y);
        }


    }

    protected override void Die()
    {
        gameObject.SetActive(false);
    }

    public void R_die()
    {
      Die();
    }
    
    public void summontoattack()
    {
        ChangeState(R_States[ResearcherStateType.Attack]);
    }
    public void PlayShot()
    {
        isarmlock = true;
        isbodylock = true;
        Armanima.Play("R_Shot");
    }

    public void Armandbodyshotend()
    {
        isarmlock = false;
        isbodylock = false;
    }

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
            }
            else if(currentDirection > 0)
            {
                armLocalScale.x = 1;             
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

        if (direction < 0) 
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
    }

    public void FlipToTargetX(float targetX)
    {
        if (isarmlock || isbodylock) return;

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

        bulletpool.Fire(dirToTarget, startPosition,false);
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
            ChangeState(r_states[ResearcherStateType.Chase]);
        }
    }

    public void WallorLedgeFlip()
    {
        if (CheckForObstacle() || CheckForLedge())
        {
            Stat.moveDistance *= -1;
            FlipResearcher(Stat.moveDistance);
            return;
        }
       
        Move();
        
    }
    public void FlipResearcher(float direction)
    {
        if (isarmlock || isbodylock) return;

        Vector3 currentScale = transform.localScale;

        if (direction > 0 && currentScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

        else if (direction < 0 && currentScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

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
        Rigid.AddForce(knockbackForce, ForceMode2D.Impulse);
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
