using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class TungTungE : MonoBehaviour, IDamageable
{
    [SerializeField] HitBox[] punches;
    [SerializeField] Transform target;
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] CapsuleCollider2D col;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Material hitFlash;
    [SerializeField] TungTungEStat stat;
    [SerializeField] Detector detector;
    [SerializeField] TungTungEState currentState;
    [SerializeField] Dictionary<string,TungTungEState> states = new();
    [SerializeField] Coroutine attack;
    CancellationTokenSource _cts;

    [SerializeField] Vector2 playerDirection;

    [SerializeField] float health;

    [SerializeField] bool facingRight;
    [SerializeField] bool attacking;
    [SerializeField] bool hitted;

    public TungTungEStat Stat => stat;
    public Transform Target => target;
    public TungTungEState CrSt => currentState;
    public Dictionary<string, TungTungEState> State => states;
    public Coroutine Attack => attack;
    public Rigidbody2D Rigid { get => rigid; set => rigid = value; }
    public CapsuleCollider2D Col { get => col; set => col = value; }

    public bool FacingRight => facingRight;
    public bool Attacking => attacking;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        detector = GetComponent<Detector>();
        ren = GetComponentInChildren<SpriteRenderer>();
        health = stat.maxHealth;
        hitted = false;
        StateInit();
        target = detector.Detect();
        
    }

    private void Update()
    {
        currentState?.Update(this);
    }

    void StateInit()
    {
        states["BattleIdle"] = new TTEBattleIdleState();
        states["Attack"] = new TTEAttackState();
        states["Chase"] = new TTEChasingState();
        states["Sway"] = new TTESwayState();
        states["Hit"] = new TTEHitState();
        states["Death"] = new TTEDeathState();
        ChangeState(states["BattleIdle"]);
    }

    void Death()
    {
        health = 0;
        ChangeState(states["Death"]);
    }

    public void ChangeState(TungTungEState state)
    {
        currentState?.Exit(this);
        currentState = state;
        currentState?.Start(this);
    }

    public void SpriteControl()
    {
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        playerDirection = dir;

        if(dir.x < 0)
        {
            facingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(dir.x > 0)
        {
            facingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void StartAttack()
    {
        attacking = true;

        StopAttack();

        _cts = new CancellationTokenSource();

        _ = LeftRight(_cts.Token);
    }

    public void StopAttack()
    {
        if(_cts != null)
        {
            Debug.LogWarning("StopAttack() 호출됨! 공격 취소 시도.");
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public float DistanceToPlayer()
    {
        float distance = transform.position.x - target.position.x;

        return Mathf.Abs(distance);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        StartCoroutine(Hit());
        if(health <= 0)
        {
            // 격퇴 시 로직
            Death();
            return;
        }
        ChangeState(states["Hit"]);
    }

    IEnumerator Hit()
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

    //IEnumerator LeftRight()
    //{
    //    punches[0].Init();
    //
    //    yield return CoroutineCasher.Wait(0.05f);
    //    punches[0].gameObject.SetActive(false);
    //
    //    yield return CoroutineCasher.Wait(0.1f);
    //    punches[1].Init();
    //    yield return CoroutineCasher.Wait(0.05f);
    //    punches[1].gameObject.SetActive(false);
    //
    //    attack = null;
    //}

    async Awaitable LeftRight(CancellationToken token)
    {
        Debug.Log($"LeftRight 시작. Token IsCancellationRequested: {token.IsCancellationRequested}");
        try
        {
            Debug.Log("공격");
            attacking = true;
            Debug.Log("뭐가 문젠데요");
            punches[0].Init();
            Debug.Log("1타");
            await Awaitable.WaitForSecondsAsync(0.05f, token);
            punches[0].gameObject.SetActive(false);
            await Awaitable.WaitForSecondsAsync(0.1f, token);
            punches[1].Init();
            Debug.Log("2타");
            await Awaitable.WaitForSecondsAsync(0.05f, token);
            punches[1].gameObject.SetActive(false);
        }
        finally
        {
            Debug.Log("공격 종료");
            punches[0].gameObject.SetActive(false);
            punches[1].gameObject.SetActive(false);
            attacking = false;
        }
    }
}
