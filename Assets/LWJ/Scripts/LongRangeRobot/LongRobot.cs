using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class LongRobot : Enemy
{
    public LongRobot_State currentstates;

    [SerializeField] private Magazine bulletPool;

    [SerializeField] Transform Player_position;
    public Transform Pl_trans => Player_position;
    [SerializeField] public Vector2 target;


    [SerializeField] Material hitFlash;
    [SerializeField] Dictionary<EnemyTypeState, LongRobot_State> LRBstate = new();
    public Dictionary<EnemyTypeState, LongRobot_State> states => LRBstate;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public LayerMask wallLayer;
    public float Movedistance => Stat.moveDistance;
    public float D_Speed => Stat.moveSpeed;
    public float wallCheckDistance = 1.0f; // 전방 벽 감지 거리
    private float nextFiretime;

    [SerializeField] bool enhanced;
    [SerializeField] bool hitted;

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
        bulletPool = GetComponentInChildren<Magazine>();
        Player_position = FindAnyObjectByType<Player>().transform;
        Stateinit();
        hitted = false;
        startPos = transform.position;
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
        LRBstate[EnemyTypeState.Idle] = new LRB_Idlestate();
        LRBstate[EnemyTypeState.Attack] = new LRB_Attackstate();
        LRBstate[EnemyTypeState.Dead] = new LRB_Deadstate();
        LRBstate[EnemyTypeState.Hit] = new LRB_Hitstate();
        LRBstate[EnemyTypeState.Chase] = new LRB_Chasestate();
        LRBstate[EnemyTypeState.Walk] = new LRB_Walkstate();
        LRBstate[EnemyTypeState.Enhance] = new LRB_EnhancedState();
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
        ChangeState(LRBstate[EnemyTypeState.Idle]);
    }

    public void ChangeState(LongRobot_State newstate)
    {
        currentstates?.Exit(this);
        currentstates = newstate;
        currentstates?.Start(this);
    }
    public void Move()
    {
        float enghancing = enhanced ? 2f : 1f;
        float M_direction = Mathf.Sign(transform.localScale.x);
        float velocityX = M_direction * D_Speed * enghancing;
        Rigid.linearVelocity = new Vector2(velocityX, Rigid.linearVelocity.y);
    }

    void Enhance()
    {
        if (currentstates is LRB_Deadstate) return;
        ChangeState(LRBstate[EnemyTypeState.Enhance]);
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, wallCheckDistance, wallLayer);

        Debug.DrawRay(origin, checkDirection * 0.5f, Color.green);

        return hit.collider != null;
    }

    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;
        Debug.Log("체력 : " + currnetHealth);

        StartCoroutine(HitFlash());

        if (currnetHealth <= 0)
        {
            ChangeState(LRBstate[EnemyTypeState.Dead]);
            return;
        }

        ChangeState(LRBstate[EnemyTypeState.Hit]);
    }

    public void FlipRobot(float direction)
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
    public float DistanceToPlayer()
    {
        if (sightrange.PlayerInSight == null)
            return 100;

        float distanceX = transform.position.x - sightrange.PlayerInSight.position.x;

        return Mathf.Abs(distanceX);
    }
    public override void Attack()
    {
        if (isattack) return;

     
        if (DistanceToPlayer() <= Stat.moveDistance)
        {
            if (Time.time >= nextFiretime)
            {
                isattack = true;
                nextFiretime = Time.time + Stat.fireCooldown;
                ChangeState(LRBstate[EnemyTypeState.Attack]);
            }
        }
        else
        {
            Debug.Log("사격 범위 이탈. 추적으로 전환.");
            ChangeState(LRBstate[EnemyTypeState.Chase]);
        }
        
    }

    public void Shoot()
    {
        Vector2 firedir = (target - (Vector2)transform.position).normalized;
        bulletPool.Fire(firedir, transform.position, false);

    }

    public void Chase()
    {
        float enhancing = enhanced ? 2f : 1f;

        float targerdirection = Pl_trans.position.x - transform.position.x;
        float directionSign = Mathf.Sign(targerdirection);

        float distance = Mathf.Abs(targerdirection);

        if (distance < 1.0f)
        {
            Rigid.linearVelocity = Vector2.zero;
        }

        if (distance > 1.0f)
        {
            FlipRobot(directionSign);
            Rigid.linearVelocity = new Vector2(directionSign * Stat.moveSpeed * enhancing, Rigid.linearVelocity.y);
        }
        else
        {
            Rigid.linearVelocity = new Vector2(0, Rigid.linearVelocity.y);
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
