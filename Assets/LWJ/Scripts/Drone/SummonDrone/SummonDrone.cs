using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum SummonDroneStateType
{
    Idle, Summon, Attack, Dead,Chase
}
public class SummonDrone : Enemy
{
    Dictionary<SummonDroneStateType, SD_State> Summonstates = new();
    public Dictionary<SummonDroneStateType, SD_State> SD_states => Summonstates;
    public SD_State currentStates { get; private set; }
    [SerializeField] private float acceleration = 10f;
    public Transform Resear_trans { get; private set; }
    public Transform Player_trans { get; private set; }

    [SerializeField] private Vector2 Offset = new Vector2(0f, 1.5f);
    public Vector2 offset => Offset;

    [SerializeField] private float ExplosionRadius = 1.5f;
    public float explosionRadius => ExplosionRadius;

    [SerializeField] private float ArriveDistance = 0.1f;
    public float arriveDistance => ArriveDistance;
    [SerializeField] public Animator animator { get; private set; }

    [SerializeField] public Material hitFlash;

    public bool isExploding { get; private set; }
    [SerializeField] bool hitted = false;

    public float SD_Speed => Stat.moveSpeed;


    protected override void Awake()
    {
        startPos = transform.position;
        animator = GetComponentInChildren<Animator>();
        Ren = GetComponentInChildren<SpriteRenderer>();
        Rigid = GetComponent<Rigidbody2D>();
        StateInit(); 
    }

    private void Update()
    {
        currentStates?.Update(this);
    }
    void StateInit()
    {
        SD_states[SummonDroneStateType.Idle] = new SD_Idlestate();
        SD_states[SummonDroneStateType.Summon] = new SD_Summonstate();
        SD_states[SummonDroneStateType.Attack] = new SD_Attackstate();
        SD_states[SummonDroneStateType.Dead] = new SD_DeadState();
        SD_states[SummonDroneStateType.Chase] = new SD_ChaseState();
    }

    private void OnEnable()
    {
        Init();
    }

    public override void Init()
    {
        currnetHealth = Stat.MaxHp;
        isExploding = false;
        hitted = false;
    }

    public void ChangeState(SD_State newstate)
    {
        if (currentStates == newstate) return;

        currentStates?.Exit(this);
        currentStates = newstate;
        currentStates?.Start(this);

    }
    public void SummonInit(Transform researcher, Transform player)
    {
        Resear_trans = researcher;
        Player_trans = player;
        ChangeState(SD_states[SummonDroneStateType.Summon]);
    }
    public override void TakeDamage(float damage)
    {
        if (isExploding) return;

        if (currentStates is SD_Summonstate) return;
        currnetHealth -= damage;
        StartCoroutine(HitFlash());
        if (currnetHealth <= 0)
        {
            ChangeState(SD_states[SummonDroneStateType.Dead]);
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


    public void DroneDie()
    {
        Die();
    }
    protected override void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("Summondrone »ç¸Á");
    }


    public void Chase()
    {
        Vector2 targetDir = (Player_trans.position - transform.position).normalized;

        Vector2 targetVelocity = targetDir * Stat.moveSpeed;

        Flip(this, targetDir.x);

        Rigid.linearVelocity = Vector2.MoveTowards(Rigid.linearVelocity,targetVelocity,acceleration * Time.deltaTime);
    }
    public void Flip(SummonDrone drone,float direction)
    {
        Vector3 currentScale = drone.transform.localScale;
       if (direction > 0)
       {
            drone.transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
       }
       else if(direction < 0)
       {
            drone.transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
       }
    }

    public override void Attack()
    {
        if (isExploding) return;
        isExploding = true;
        Debug.Log("Æø¹ß ½ÃÀÛ");
        StartCoroutine(Explosion_timer());
    }

    
    IEnumerator Explosion_timer()
    {
        yield return CoroutineCasher.Wait(0.5f);
        ChangeState(SD_states[SummonDroneStateType.Dead]);
       
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

    }
}
