using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DroneStateType
{
    Idle, Attack, Dead, Hit, Walk, Chase, Enhance, Return
}
public class Drone : Enemy
{
    public DroneState currentstates;

    [SerializeField] private Magazine bulletPool;

    [SerializeField] Transform Player_position;
    public Transform Pl_trans => Player_position;
    [SerializeField] Vector2 target;


    [SerializeField] Material hitFlash;
    [SerializeField] Dictionary<DroneStateType, DroneState> Dronestate = new(); 
    public Dictionary<DroneStateType, DroneState> State => Dronestate;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public LayerMask wallLayer;
    public float Movedistance => Stat.moveDistance;
    public float D_Speed => Stat.moveSpeed;
    public float wallCheckDistance = 1.0f; // 전방 벽 감지 거리
    private float nextFiretime;

    [SerializeField] bool enhanced;
    [SerializeField] bool hitted;

    [SerializeField] private PatrolPoints patrolPath; 
    public PatrolPoints PatrolPath => patrolPath;

    public int currentPatrolIndex = -1;
    public SightRange sightrange { get; private set; }
    public AimRange aimrange { get; private set; } 
    public Animator animator { get; private set; }
    public bool Enhanced { get => enhanced; set => enhanced = value; }
    public bool isattack { get; set; }

    protected override void Awake()
    {
       
        sightrange = GetComponent<SightRange>();
        aimrange = GetComponent<AimRange>();
        ren = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        Rigid = GetComponent<Rigidbody2D>();
        patrolPath = GetComponentInChildren<PatrolPoints>();
        if (patrolPath != null)
        {
            patrolPath.Initialize();
        }
        else
        {
            Debug.LogError("자식 오브젝트에서 PatrolPoints를 찾을 수 없습니다!");
        }
        Stateinit();
        currnetHealth = Stat.MaxHp;
        hitted = false;
        startPos = transform.position;  
    }

    void Start()
    {
        
    }

    private void OnEnable()
    {
        Init();
        EventManager.Subscribe(Event.Enemy_Enhance, Enhance);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(Event.Enemy_Enhance, Enhance);
    }
    private void Stateinit()
    {
        Dronestate[DroneStateType.Idle] = new D_Idlestate();
        Dronestate[DroneStateType.Attack] = new D_Attackstate();
        Dronestate[DroneStateType.Dead] = new D_Deadstate();
        Dronestate[DroneStateType.Hit] = new D_Hitstate();
        Dronestate[DroneStateType.Chase] = new D_Chasestate();
        Dronestate[DroneStateType.Walk] = new D_Walkstate();
        Dronestate[DroneStateType.Enhance] = new D_EnhancedDroneState();
        Dronestate[DroneStateType.Return] = new D_Returnstate();
    }
    void Update()
    {
        currentstates?.Update(this);
    }

    public override void Init() 
    {
        currnetHealth = Stat.MaxHp;
        Rigid.linearVelocity = Vector2.zero;
        transform.position = startPos;
        isattack = false;
        ChangeState(Dronestate[DroneStateType.Idle]);
    }

    public void ChangeState(DroneState drone)
    {
        currentstates?.Exit(this);
        currentstates = drone;
        currentstates?.Start(this);  
    }
    public void Move(Vector3 nextpos)
    {
        float distance = Vector2.Distance(transform.position, nextpos);
        if (distance > 0.2f) 
        {
            Vector2 moveDir = (nextpos - transform.position).normalized;
            float enhancing = Enhanced ? 2f : 1f;
            Rigid.linearVelocity = moveDir * Stat.moveSpeed * enhancing;
        }
        else
        {
            Rigid.linearVelocity = Vector2.zero;
            ChangeState(State[DroneStateType.Idle]);
        }
    }

    void Enhance()
    {
        if (currentstates is D_Deadstate) return;
        ChangeState(Dronestate[DroneStateType.Enhance]);
        StartCoroutine(Enhancing());
    }
    IEnumerator Enhancing()
    {
        float t = 0;
        Color origin = ren.color;
        Color target = Color.red;

        while (t < 1f)
        {
            t += Time.deltaTime;
            float progress = t / 1.5f;

            ren.color = Color.Lerp(origin, target, progress);

            yield return null;
        }
    }
    public bool CheckForObstacle()
    {
        float lookDir = Mathf.Sign(transform.localScale.x);
        Vector2 checkDirection = (lookDir > 0) ? Vector2.right : Vector2.left;

        Vector2 origin = (Vector2)transform.position + (checkDirection * 0.3f);


        RaycastHit2D hit = Physics2D.CircleCast(origin, wallCheckDistance, checkDirection, 0.1f, wallLayer);

        Debug.DrawRay(origin, checkDirection * 0.5f, Color.green);

        return hit.collider != null;
    }

    public void ReturnToStartPoint()
    {
        Vector3 targetPos = patrolPath.GetRandomPoint();
        float distance = Vector2.Distance(transform.position, targetPos);

        if (distance > 0.2f)
        {
            Vector2 moveDir = ((Vector2)patrolPath.GetRandomPoint() - (Vector2)transform.position).normalized;

            FlipDrone(moveDir.x);

            Rigid.linearVelocity = moveDir * Stat.moveSpeed;
        }
        else
        {
            Rigid.linearVelocity = Vector2.zero;
            ChangeState(Dronestate[DroneStateType.Idle]);
        }
    }

    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;
        Debug.Log("체력 : " + currnetHealth);

        StartCoroutine(HitFlash());

        if (currnetHealth <= 0)
        {
            ChangeState(Dronestate[DroneStateType.Dead]);
            return;
        }

        ChangeState(Dronestate[DroneStateType.Hit]);

    }

    public void FlipDrone(float direction)
    {
        Vector3 currentScale = transform.localScale;
        float newX = Mathf.Abs(currentScale.x) * Mathf.Sign(direction);
        transform.localScale = new Vector3(newX, currentScale.y, currentScale.z);
    }

    public void waitgameobjectfalse()
    {
        StartCoroutine(Dead());

    }

    IEnumerator Dead()
    {
        yield return CoroutineCasher.Wait(1.0f);
        gameObject.SetActive(false);
    }
    protected override void Die() { }

    public override void Attack()
    {
        if (isattack) return;

        if (!isattack)
        {
            if (aimrange != null && aimrange.IsPlayerInSight)
            {
                if (Time.time >= nextFiretime)
                {
                    isattack = true;
                    nextFiretime = Time.time + Stat.fireCooldown;

                    Resetplayerposition();
                    ChangeState(Dronestate[DroneStateType.Attack]);
                }
            }
            else
            {
                Debug.Log("사격 범위 이탈. 추적으로 전환.");
                ChangeState(Dronestate[DroneStateType.Chase]);
            }        
        }
    }
    public void Resetplayerposition()
    {
        target = Pl_trans.position;
        float direction = target.x - transform.position.x;
        FlipDrone(direction);
    }

    public void Shoot()
    {
        Vector2 firedir = (target - (Vector2)transform.position).normalized;
        bulletPool.Fire(firedir, transform.position, false);

    }

    public void Chase()
    {
        float enhancing = enhanced ? 2f : 1f;   

        Vector2 direction = (Pl_trans.position - transform.position).normalized;

        FlipDrone(direction.x);

        Rigid.linearVelocity = direction * Stat.moveSpeed * enhancing;

        float distance = Vector2.Distance(transform.position, Pl_trans.position);

        if (distance < 1.0f)
        {
            Rigid.linearVelocity = Vector2.zero;
        }
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
