using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour
{
    public DroneState[] droneStates = new DroneState[3];
    DroneState currentstates;
    public Transform Resear_trans;
    public Transform Player_trans;
  
    bool isWait = false;
    public Vector2 offset = new Vector2(0f, 1.5f);

    [SerializeField] LayerMask playerLayer;
    [SerializeField] float explosionRadius = 1.5f;
    [SerializeField] LayerMask damagelayer;
    [SerializeField] public float D_speed = 3f;
    SpriteRenderer spriteRenderer;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public float Movedistance = 1f;
    public float D_Speed = 2f;
    public float horizontalDirection = 1f;
    public float wallCheckDistance = 0.5f; // 전방 벽 감지 거리
    public SightRange sightRange;

    void Awake()
    {
        droneStates[0] = new D_Idlestate();
        droneStates[1] = new D_Attackstate();
        droneStates[2] = new D_Summonstate();
        ChangeState(droneStates[0]);
        sightRange = GetComponent<SightRange>();    
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    void Start()
    {
        
    }


    void Update()
    {
        currentstates?.Update(this);

    }

    public void ChangeState(DroneState drone)
    {
        currentstates?.Exit(this);
        currentstates = drone;
        currentstates?.Start(this);  
    }

    public void SummonInit(Transform researcher, Transform player)
    {
        Resear_trans = researcher;
        Player_trans = player;
        ChangeState(droneStates[2]);  
    }

   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("드론이 플레이어와 충돌했습니다.");
            Destroy(this.gameObject);
        }

    }


    #region 드론 공격 대기 코루틴
    public void WaitDroneandattackstate()
    {
        if (isWait) return;

        isWait = true;
        StartCoroutine(Waitone());
    }
    IEnumerator Waitone()
    {
        yield return CoroutineCasher.Wait(1f);
        ChangeState(droneStates[1]);
        isWait = false;
    }


    #endregion

    #region 드론 자폭
    public void StartExplosionTimer()
    {
        StartCoroutine(Explosion_timer());
    }
    IEnumerator Explosion_timer()
    {
        yield return CoroutineCasher.Wait(3f);
        Debug.Log("드론 폭발");
        PerformExplosion();
        Destroy(this.gameObject);
    }
    void PerformExplosion()
    {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damagelayer);

        foreach (Collider2D col in objectsInRange)
        {
            Debug.Log($"{col.gameObject.name} 폭발 데미지 받음.");
        }

    }

    #endregion
    public void SetDroneActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
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
}
