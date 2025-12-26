using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuardStateType
{
    Idle, Walk, Chase, Attack, Hit, Dead
}
public class Security_Guard : Enemy
{
    [SerializeField] Dictionary<GuardStateType, Security_State> Guardstates = new();
    public Dictionary<GuardStateType, Security_State> states => Guardstates;

    private Security_State currentState;

    [SerializeField] Magazine bulletpool;
    [SerializeField] Material hitFlash;

    [SerializeField] HitBox impact;

    [SerializeField] bool hitted;

    public LayerMask groundLayer;
    public LayerMask wallLayer;
    [SerializeField] public Animator animator { get; private set; }
    public SightRange sightRange { get; private set; }

    [SerializeField] bool Isattack; 
    public Transform target;
    private float wallCheckDistance = 1f;
    private float groundCheckDistance = 1f;
    public bool isattack { get => Isattack; set => Isattack = value; }
    public float nextFireTime;
    protected override void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        ren = GetComponentInChildren<SpriteRenderer>();
        sightRange = GetComponent<SightRange>();
        Rigid = GetComponent<Rigidbody2D>();
        StateInit();
    }

    public void OnEnable()
    {
        Init();
    }
    public override void Init()
    {
        currnetHealth = Stat.MaxHp;
        isattack = false;
        ChangeState(Guardstates[GuardStateType.Idle]);
    }
    
    public void StateInit()
    {
        Guardstates[GuardStateType.Idle] = new Security_Idle();
        Guardstates[GuardStateType.Walk] = new Security_Walk();
        Guardstates[GuardStateType.Chase] = new Security_Chase();
        Guardstates[GuardStateType.Attack] = new Security_Attack();
        Guardstates[GuardStateType.Hit] = new Security_Hit();
        Guardstates[GuardStateType.Dead] = new Security_Death();
    }
    void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(Security_State newstate)
    {

        currentState?.Exit(this);
        currentState = newstate;
        currentState.Start(this);
    }

    public override void Attack()
    {
        StartCoroutine(falsehitbox());  
    }

    IEnumerator falsehitbox()
    {
        impact.Init(false);
        yield return CoroutineCasher.Wait(0.2f);
        impact.gameObject.SetActive(false);
        isattack = false;
    }

    public void FlipGuard(float direction)
    {
        Vector3 currentScale = transform.localScale;
        float newX = Mathf.Abs(currentScale.x) * Mathf.Sign(direction);
        transform.localScale = new Vector3(newX, currentScale.y, currentScale.z);
    }

    public void Chase()
    {
        if (target == null) return;

        float targerdirection = target.position.x - transform.position.x;
        float directionSign = Mathf.Sign(targerdirection);

        float distance = Mathf.Abs(targerdirection);

        if (distance > 1.0f)
        {
            FlipGuard(directionSign);
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

    public void guarddie()
    {
        Die();
    }
    public void Move()
    {
        float M_direction = Mathf.Sign(transform.localScale.x);
        float velocityX = M_direction * Stat.moveSpeed;
        Rigid.linearVelocity = new Vector2(velocityX, Rigid.linearVelocity.y);
    }
    public bool CheckForObstacle()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 checkDirection = (direction > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, wallCheckDistance, wallLayer);

        return hit.collider != null;
    }
    public float DistanceToPlayer()
    {
        if (sightRange.PlayerInSight == null)
            return 100;

        float distanceX = transform.position.x - sightRange.PlayerInSight.position.x;

        return Mathf.Abs(distanceX);
    }
    public bool CheckForLedge()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector3 footPosition = transform.position;
        footPosition.x += direction * 0.3f;

        RaycastHit2D hit = Physics2D.Raycast(footPosition, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider == null;
    }
    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;

        StartCoroutine(HitFlash());

        if (currnetHealth <= 0)
        {
            ChangeState(Guardstates[GuardStateType.Dead]);
            return;
        }
        ChangeState(Guardstates[GuardStateType.Hit]);
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
