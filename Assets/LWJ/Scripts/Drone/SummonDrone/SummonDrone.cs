using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum SummonDroneStateType
{
    Idle, Summon, Attack, Dead
}
public class SummonDrone : Enemy
{
    Dictionary<SummonDroneStateType, SD_State> Summonstates = new();
    public Dictionary<SummonDroneStateType, SD_State> SD_states => Summonstates;

    public Transform Resear_trans;
    public Transform Player_trans;

    public SD_State currentStates;

    public Vector2 offset = new Vector2(0f, 1.0f);

    [SerializeField] public SightRange sightRange;
    [SerializeField] public Animator animator;
    [SerializeField] public Material hitFlash;
    [SerializeField] LayerMask damagelayer;
    [SerializeField] bool hitted = false;
    bool isExploding = false;

    [SerializeField] float explosionRadius = 1.5f;
    public float SD_Speed;
    public float Arriveposition = 0.1f;
    public float SD_direction;
    protected override void Awake()
    {
        startPos = transform.position;
        sightRange = GetComponent<SightRange>();
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
    }

    private void OnEnable()
    {
        Init();

    }
    // 초기화 (원하면 override 가능)
    public override void Init()
    {
        currnetHealth = Stat.MaxHp;
        SD_Speed = Stat.moveSpeed;
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
    // 알아서 수정할 것
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
    // 공통 사망 처리

    public void DroneDie()
    {
        Die();
    }
    protected override void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("Summondrone 사망");
    }

    public void R_directiontoDrone()
    {
        float researcherSign = Mathf.Sign(Resear_trans.localScale.x);
        float currentAbsX = Mathf.Abs(transform.localScale.x);
        float newScaleX = currentAbsX * researcherSign;

        transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
    }
    public void Chase()
    {
        Vector3 targetPosition = Player_trans.position;


        transform.position = Vector3.MoveTowards(transform.position,targetPosition,SD_Speed * Time.deltaTime);

        Vector2 directionToPlayer = Player_trans.position - transform.position;
        Flip(this, directionToPlayer.x);
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
        StartCoroutine(Explosion_timer());
    }

    
    IEnumerator Explosion_timer()
    {
        yield return CoroutineCasher.Wait(3f);
        ChangeState(SD_states[SummonDroneStateType.Dead]);
        PerformExplosion();
       
    }

    void PerformExplosion()
    {

        Collider2D hitCollider = Physics2D.OverlapCircle(transform.position, explosionRadius, damagelayer);

        if (hitCollider != null)
        {
            Player player = hitCollider.GetComponent<Player>();

            if (player != null)
            { 
                player.Hit(1);
            }
        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

    }
}
