using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;

public class EnhancableMelee : Enemy
{
    [SerializeField] Animator animator;
    [SerializeField] Transform target;
    [SerializeField] CapsuleCollider2D col;
    [SerializeField] Material hitFlash;
    [SerializeField] HitBox impact;
    [SerializeField] Detector detector;
    [SerializeField] EnhancableMeleeState currentState;
    [SerializeField] Dictionary<string, EnhancableMeleeState> state = new();
    CancellationTokenSource _cts;
    MaterialPropertyBlock mpb;
    Coroutine hit;

    [SerializeField] LayerMask groundMask;
    [SerializeField] Vector2 playerDirection;

    [SerializeField] bool attacking;
    [SerializeField] bool enhanced;
    [SerializeField] bool isDead;

    public Transform Target => target;
    public EnhancableMeleeState CrSt => currentState;
    public HitBox Impact => impact;
    public Dictionary<string, EnhancableMeleeState> State => state;
    public Animator AniCon { get => animator; set => animator = value; }
    public CapsuleCollider2D Col { get => col; set => col = value; }

    public bool Enhanced { get => enhanced; set => enhanced = value; }
    public bool Attacking { get => attacking; set => attacking = value; }
    public bool IsDead { get => isDead; set => isDead = value; }

    protected override void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        detector = GetComponent<Detector>();
        ren = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        mpb = new MaterialPropertyBlock();
        ren.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0.5f);
        ren.SetPropertyBlock(mpb);
        currnetHealth = stat.MaxHp;
        StateInit();
    }

    private void OnEnable()
    {
        EventManager.Subscribe(Event.Enemy_Enhance, Enhance);
    }

    private void OnDisable()
    {
        EventManager.Subscribe(Event.Enemy_Enhance, Enhance);
    }

    private void Update()
    {
        target = detector.Detect();
        currentState?.Update(this);
        
    }

    void StateInit()
    {
        state["Idle"] = new EMIdleState();
        state["Patrol"] = new EMPatrolState();
        state["Chase"] = new EMChasingState();
        state["Attack"] = new EMAttackState();
        state["Enhance"] = new EMEnhanceState();
        state["Hit"] = new EMHitState();
        state["Death"] = new EMDestroyState();

        ChangeState(state["Idle"]);
    }

    void Enhance()
    {
        if (isDead || currentState is EMDestroyState)
            return;
        ChangeState(state["Enhance"]);
        StartCoroutine(Enhancing());
    }

    void Hit()
    {
        if(hit != null)
        {
            StopCoroutine(hit);
        }
        hit = StartCoroutine(HitFlash());
    }

    public bool CheckWall()
    {
        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.2f;

        Vector2 dir = facingRight ? Vector2.right : Vector2.left;

        Vector2 origin = (Vector2)transform.position + new Vector2(0, bottomY);
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, radius + 0.5f, groundMask);
        Debug.DrawRay(origin, dir * (radius + 0.5f), Color.blue);

        return hit.collider != null;
    }

    public bool CheckEdge()
    {
        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.2f;

        Vector2 origin = (Vector2)transform.position + new Vector2(0, bottomY);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, radius + 0.5f, groundMask);
        Debug.DrawRay(origin, Vector2.down * (radius + 0.5f), Color.red);

        return hit.collider == null;
    }

    public void SpriteControl(bool faceRight = false)
    {
        if (target != null)
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            playerDirection = dir;

            if (dir.x < 0)
            {
                facingRight = false;
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (dir.x > 0)
            {
                facingRight = true;
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            facingRight = faceRight;
            if(faceRight)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    public override void Init()
    {
        
    }

    public float DistanceToPlayer()
    {
        if (target == null)
            return 100;

        float distanceX = transform.position.x - target.position.x;

        return Mathf.Abs(distanceX);
    }

    protected override void Die()
    {
        StartCoroutine(Weaked());
        ChangeState(state["Death"]);
    }

    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;
        Hit();
        if(currnetHealth <= 0)
        {
            currnetHealth = 0;
            Die();
            return;
        }
        ChangeState(state["Hit"]);
    }

    public void ChangeState(EnhancableMeleeState state)
    {
        currentState?.Exit(this);
        currentState = state;
        currentState?.Start(this);
    }

    public bool CheckTerrain()
    {
        return CheckWall() || CheckEdge();
    }

    public override void Attack()
    {
        StopAttack();
        _cts = new CancellationTokenSource();
        _ = InitiateAttack(_cts.Token);
    }

    public void StopAttack()
    {
        if(_cts != null)
        {
            _cts.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    IEnumerator HitFlash()
    {
        Material origin = ren.material;
        
        ren.material = hitFlash;
        
        mpb.SetFloat("_FlashAmount", 0.5f);
        ren.SetPropertyBlock(mpb);

        float t = 0;
        float duration = 0.15f;

        while (t <= duration)
        {
            t += Time.deltaTime;
            float flash = Mathf.SmoothStep(0.5f, 0f, t / duration);
            mpb.SetFloat("_FlashAmount", flash);
            ren.SetPropertyBlock(mpb);
            yield return null;
        }

        ren.material = origin;
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

    IEnumerator Weaked()
    {
        float t = 0;
        Color origin = ren.color;
        Color target = Color.white;

        while(t < 1f)
        {
            t += Time.deltaTime;
            float progress = t / 1.5f;

            ren.color = Color.Lerp(origin, target, progress);

            yield return null;
        }
    }


    async Awaitable InitiateAttack(CancellationToken token)
    {
        try
        {
            attacking = true;
            impact.Init(enhanced);
            await Awaitable.WaitForSecondsAsync(0.1f);
            impact.gameObject.SetActive(false);
        }
        finally
        {
            impact.gameObject.SetActive(false);
            attacking = false;
        }
    }
}
