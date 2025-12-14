using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum SummonDroneStateType
{
    Idle, Summon, Attack, Dead
}
public class SummonDrone : Enemy
{
    [SerializeField] Dictionary<SummonDroneStateType, SD_State> Summonstates = new();

    public Dictionary<SummonDroneStateType, SD_State> SD_states => Summonstates;

    public Transform Resear_trans;
    public Transform Player_trans;

    public SD_State currentStates;

    public Vector2 offset = new Vector2(0f, 1.5f);

    [SerializeField] public SightRange sightRange;
    [SerializeField] public Animator animator;
    public float SD_Speed;
    public float Arriveposition = 0.1f;
    protected override void Awake()
    {
        sightRange = GetComponent<SightRange>();
        animator = GetComponentInChildren<Animator>();
        StateInit(); 
        Init();
    }

    private void Update()
    {
        currentStates.Update(this);
    }
    void StateInit()
    {
        SD_states[SummonDroneStateType.Idle] = new SD_Idlestate();
        SD_states[SummonDroneStateType.Summon] = new SD_Summonstate();
        SD_states[SummonDroneStateType.Attack] = new SD_Attackstate();
        SD_states[SummonDroneStateType.Dead] = new SD_DeadState();
    }

    // 초기화 (원하면 override 가능)
    public override void Init()
    {
        SD_Speed = Stat.moveSpeed;
        ChangeState(SD_states[SummonDroneStateType.Idle]);
    }

    public void ChangeState(SD_State newstate)
    {
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

    }

    // 공통 사망 처리
    protected override void Die()
    {

    }

    public override void Attack()
    {

    }

    public void StartExplosionTimer()
    {
        StartCoroutine(Explosion_timer());
    }
    IEnumerator Explosion_timer()
    {
        yield return CoroutineCasher.Wait(3f);
        Debug.Log("드론 폭발");
        animator.Play("Dead");
        //PerformExplosion();
       
    }

    void PerformExplosion()
    {
        //if (!isattack) return;

        //Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damagelayer);
        //
        //foreach (Collider2D col in objectsInRange)
        //{
        //    Debug.Log($"{col.gameObject.name} 폭발 데미지 받음.");
        //    Player player = col.GetComponent<Player>();
        //
        //    if (player != null)
        //    {
        //        player.Hit(1); 
        //    }   
        //}

    }
}
