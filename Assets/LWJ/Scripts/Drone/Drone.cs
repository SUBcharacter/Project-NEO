using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Drone : Enemy
{
    public DroneState currentstates;

    [SerializeField] private Magazine bulletPool;

    [SerializeField] Transform Player_position;
    public Transform Pl_trans => Player_position;
 
    [SerializeField] private Vector3 Target;

    public Vector3 target => Target;
    [SerializeField] Material hitFlash;
    [SerializeField] Dictionary<EnemyTypeState, DroneState> Dronestate = new(); 
    public Dictionary<EnemyTypeState, DroneState> State => Dronestate;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public LayerMask wallLayer;
    public float wallCheckDistance = 1.5f; // 전방 벽 감지 거리

    [SerializeField] bool enhanced;
    [SerializeField] bool hitted;

    [SerializeField] private PatrolPoints patrolPath; 
    public PatrolPoints PatrolPath => patrolPath;

    public int currentPatrolIndex = -1;
    public SightRange sightrange { get; private set; }
    public Animator animator { get; private set; }
    public bool Enhanced { get => enhanced; set => enhanced = value; }
    public bool isattack { get; set; }

    protected override void Awake()
    {
        sightrange = GetComponent<SightRange>();
        ren = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        Rigid = GetComponent<Rigidbody2D>();
        patrolPath = FindAnyObjectByType<PatrolPoints>();
        if (patrolPath != null)
        {
            patrolPath.Initialize();
        }

        Player_position= FindAnyObjectByType<Player>().transform;
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
        Dronestate[EnemyTypeState.Idle] = new D_Idlestate();
        Dronestate[EnemyTypeState.Attack] = new D_Attackstate();
        Dronestate[EnemyTypeState.Dead] = new D_Deadstate();
        Dronestate[EnemyTypeState.Hit] = new D_Hitstate();
        Dronestate[EnemyTypeState.Chase] = new D_Chasestate();
        Dronestate[EnemyTypeState.Walk] = new D_Walkstate();
        Dronestate[EnemyTypeState.Enhance] = new D_EnhancedDroneState();
        Dronestate[EnemyTypeState.Return] = new D_Returnstate();
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
        ChangeState(Dronestate[EnemyTypeState.Idle]);
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
            ChangeState(State[EnemyTypeState.Idle]);
        }
    }

    void Enhance()
    {
        if (currentstates is D_Deadstate) return;
        ChangeState(Dronestate[EnemyTypeState.Enhance]);
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

        RaycastHit2D hit = Physics2D.Raycast(origin, checkDirection, wallCheckDistance, wallLayer);


        return hit.collider != null;
    }

    public void ReturnToStartPoint()
    {

        float distance = Vector2.Distance(transform.position, startPos);

        if (distance > 0.3f)
        {
            Vector2 moveDir = ((Vector2)startPos - (Vector2)transform.position).normalized;
            FlipDrone(moveDir.x);
            Rigid.linearVelocity = moveDir * Stat.moveSpeed;
        }
        else
        {
            Rigid.linearVelocity = Vector2.zero;
            ChangeState(Dronestate[EnemyTypeState.Idle]);
        }
    }

    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;
        Debug.Log("체력 : " + currnetHealth);

        StartCoroutine(HitFlash());

        if (currnetHealth <= 0)
        {
            ChangeState(Dronestate[EnemyTypeState.Dead]);
            return;
        }

        ChangeState(Dronestate[EnemyTypeState.Hit]);

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

     
        if (DistanceToPlayer() <= Stat.moveDistance)
        {
            ChangeState(State[EnemyTypeState.Attack]);
        }
        else
        {     
             ChangeState(State[EnemyTypeState.Chase]);

        }           
       


    }
    public float DistanceToPlayer()
    {
        if (sightrange.PlayerInSight == null)
            return 100;

        float distanceX = transform.position.x - sightrange.PlayerInSight.position.x;

        return Mathf.Abs(distanceX);
    }
    public void Resetplayerposition()
    {
        if (sightrange.PlayerInSight != null)
        {
            Target = sightrange.PlayerInSight.position; 
            float direction = Target.x - transform.position.x;
            FlipDrone(direction);
        }
    }

    public void Shoot()
    {
        Vector2 firedir = ((Vector2)Target - (Vector2)transform.position).normalized;
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
