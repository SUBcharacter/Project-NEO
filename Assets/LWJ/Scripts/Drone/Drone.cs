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

    public float Movedistance;
    public float D_Speed => Stat.moveSpeed;
    public float wallCheckDistance = 0.5f; // 전방 벽 감지 거리
    private float nextFiretime;

    public SightRange sightrange { get; private set; }
    public AimRange aimrange { get; private set; } 
    public Animator animator { get; private set; }

    [SerializeField] bool hitted;
    public bool isattack { get; set; }

    protected override void Awake()
    {
        sightrange = GetComponent<SightRange>();
        aimrange = GetComponent<AimRange>();
        ren = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        Rigid = GetComponent<Rigidbody2D>();
        Statinit();
        currnetHealth = Stat.MaxHp;
        Movedistance = Stat.moveDistance;
        hitted = false;
        startPos = transform.position;
       
    }

    void Start()
    {
        
    }

    private void OnEnable()
    {
        Init();
    }
    private void Statinit()
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
    public void Move()
    {
        Movedistance = Mathf.Sign(transform.localScale.x);
        float velocityX = Movedistance * Stat.moveSpeed;
        Rigid.linearVelocity = new Vector2(velocityX, Rigid.linearVelocity.y);
    }

    public bool CheckForObstacle()
    { 
        Vector2 checkDirection = (Movedistance > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, wallCheckDistance, wallLayer);

        return hit.collider != null;
    }

    public void ReturnToStartPoint()
    {
        float distance = Vector2.Distance(transform.position, startPos);

        if (distance > 0.1f)
        {
            Vector2 moveDir = ((Vector2)startPos - (Vector2)transform.position).normalized;

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

    protected override void Die() { }

    public override void Attack()
    {
        if (isattack) return;

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
            if(!isattack)
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
        Vector2 direction = (Pl_trans.position - transform.position).normalized;

        FlipDrone(direction.x);

        Rigid.linearVelocity = direction * Stat.moveSpeed;

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
