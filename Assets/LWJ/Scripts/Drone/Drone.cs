using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum DroneStateType
{
    Idle, Attack, Dead, Hit, Walk, Chase, Enhance
}
public class Drone : Enemy
{
    public DroneState[] droneStates = new DroneState[3];
    DroneState currentstates;
    public Transform Resear_trans;
    public Transform Player_trans;
  
    bool isWait = false;
    public bool isattack = false;
    public Vector2 offset = new Vector2(0f, 1.0f);

    [SerializeField] LayerMask playerLayer;

    private Dictionary<DroneStateType, DroneState> Dronestate = new(); 
    Dictionary<DroneStateType, DroneState> droneState => Dronestate;

    SpriteRenderer spriteRenderer;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public float Movedistance;
    public float D_Speed;
    public float horizontalDirection = 1f;
    public float wallCheckDistance = 0.5f; // 전방 벽 감지 거리

    private SightRange sightRange;
    public SightRange sightrange;

    public Animator animator;

    [SerializeField] bool hitted;

    protected override void Awake()
    {
       
        sightrange = GetComponent<SightRange>();    
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        Statinit();
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
        droneState[DroneStateType.Idle] = new D_Idlestate();
        droneState[DroneStateType.Attack] = new D_Attackstate();
        droneState[DroneStateType.Dead] = new D_Deadstate();
        droneState[DroneStateType.Hit] = new D_Hitstate();
        droneState[DroneStateType.Chase] = new D_Chasestate();
        droneState[DroneStateType.Walk] = new D_Walkstate();
        droneState[DroneStateType.Enhance] = new D_EnhancedDroneState();

    }
    void Update()
    {
        currentstates?.Update(this);
    }

    public override void Init() 
    {
        //currnetHealth = Stat.MaxHp;
        //D_Speed = Stat.moveSpeed;
        ChangeState(droneStates[0]);
        hitted = false;
    }
    public void ChangeState(DroneState drone)
    {
        currentstates?.Exit(this);
        currentstates = drone;
        currentstates?.Start(this);  
    }

    public override void TakeDamage(float damage)
    {
        Debug.Log("드론이 데미지를 입었습니다.");

        currnetHealth -= damage;
        if(currnetHealth <= 0)
        {
            Debug.Log("드론 파괴");
           gameObject.SetActive(false);
        }   
    }
 
    public void SetDroneActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void FlipDrone(Drone drone, float direction)
    {

        Vector3 currentScale = drone.transform.localScale;


        if (direction > 0 && currentScale.x < 0)
        {
            drone.transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

        else if (direction < 0 && currentScale.x > 0)
        {
            drone.transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    protected override void Die() { }

    public override void Attack()
    {

    }
}
