using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour
{
    public DroneState[] droneStates = new DroneState[5];
    DroneState currentstates;
    public Transform Resear_trans;
    public Transform Player_trans;
    [SerializeField] public float D_speed = 3f;
    bool isWait = false;
    public Vector2 offset = new Vector2(0f, 1.5f);
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float explosionRadius = 1.5f;
    [SerializeField] LayerMask damagelayer; 
    void Awake()
    {
        droneStates[0] = new D_Idlestate();
        droneStates[1] = new D_Attackstate();
      
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

    public void Init(Transform researcher, Transform player)
    {
        Resear_trans = researcher;
        Player_trans = player;
        ChangeState(droneStates[0]);  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("드론이 플레이어와 충돌했습니다.");
            Destroy(this.gameObject);
        }

    }
    void PerformExplosion()
    {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damagelayer);

        foreach (Collider2D col in objectsInRange)
        {
            Debug.Log($"{col.gameObject.name} 폭발 데미지 받음.");
        }

    }
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

    IEnumerator Explosion_timer()
    {
        yield return CoroutineCasher.Wait(3f);
        PerformExplosion();
        Destroy(this.gameObject);
    }

    public void SetDroneActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
