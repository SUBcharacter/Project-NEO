using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Bisili : MonoBehaviour, IDamageable
{
    [SerializeField] HitBox swing;
    [SerializeField] Transform target;
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] CapsuleCollider2D col;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Detector detector;
    [SerializeField] BisiliStat stat;
    [SerializeField] BisiliState currentState;
    [SerializeField] Dictionary<string, BisiliState> states = new();
    CancellationTokenSource _cts;

    [SerializeField] Color baseColor;

    [SerializeField] float health;

    [SerializeField] bool attacking;
    [SerializeField] bool facingRight;

    public Transform Target => target;
    public BisiliState CrSt => currentState;
    public BisiliStat Stat => stat;
    public Dictionary<string, BisiliState> State => states;
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
        target = detector.Detect();
        health = stat.maxHealth;
        baseColor = ren.color;
        StateInit();
        ChangeState(states["BattleIdle"]);
    }

    private void Update()
    {
        currentState?.Update(this);
    }

    void StateInit()
    {
        states["BattleIdle"] = new BSBattleIdleState();
        states["Chase"] = new BSChasingState();
        states["Sway"] = new BSSwayState();
        states["Attack"] = new BSAttackState();
        states["Hit"] = new BSHitState();
        states["Death"] = new BSDeathState();

        ChangeState(states["BattleIdle"]);
    }

    void Death()
    {
        health = 0;
        ChangeState(states["Death"]);
    }

    public void SpriteControl()
    {
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;

        if(dir.x < 0)
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

    public void ChangeState(BisiliState state)
    {
        currentState?.Exit(this);
        currentState = state;
        currentState?.Start(this);
    }

    public float DistanceToPlayer()
    {
        float distance = transform.position.x - target.position.x;
        return Mathf.Abs(distance);
    }

    public void StartAttack()
    {
        attacking = true;
        StopAttack();
        _cts = new CancellationTokenSource();
        _ = Swing(_cts.Token);
    }

    public void StopAttack()
    {
        if (_cts != null)
        { 
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        StartCoroutine(Hit());
        if(health <= 0)
        {
            Death();
            return;
        }
        ChangeState(states["Hit"]);
    }
    IEnumerator Hit()
    {
        ren.color = Color.red;

        yield return CoroutineCasher.Wait(0.05f);

        ren.color = baseColor;
    }
    
    async Awaitable Swing(CancellationToken token)
    {
        try
        {
            attacking = true;
            swing.Init();
            await Awaitable.WaitForSecondsAsync(0.15f, token);
            swing.gameObject.SetActive(false);
        }
        finally
        {
            attacking = false;
            swing.gameObject.SetActive(false);
        }
    }
    
}
