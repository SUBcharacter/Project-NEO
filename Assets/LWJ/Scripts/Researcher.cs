using System.Collections;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
public class Researcher : MonoBehaviour
{
    [SerializeField] public Transform[] dronespawnpoints;
    [SerializeField] public Transform Player_Trans;

    [SerializeField] public GameObject Bullet_prefab;
    [SerializeField] public GameObject D_prefab;

    public ResearcherState[] R_States = new ResearcherState[3];
    public ResearcherState currentStates;

    public SightRange sightRange;

    public bool isDroneSummoned = false;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private SpriteRenderer spriteRenderer;

    public float R_Speed = 2f;
    public float Movedistance = 1f;
    private float wallCheckDistance = 0.5f;
    private float groundCheckDistance = 0.8f;
    private float WaitTimer = 5f;
    private float statetime;
    void Awake()
    {
        sightRange = GetComponent<SightRange>();    
        spriteRenderer = GetComponent<SpriteRenderer>();
        R_States[0] = new R_IdleState();
        R_States[1] = new R_SummonDroneState();
        R_States[2] = new R_Attackstate();
        ChangeState(R_States[0]);

    }

    void Start()
    {
        statetime = Time.time + WaitTimer;
    }

    void Update()
    {
        currentStates?.Update(this);
    }

    public void ChangeState(ResearcherState newState)
    {
       currentStates?.Exit(this);
       currentStates = newState;
       currentStates?.Start(this);
    }
    
    public void ShootBullet(Vector2 dir)
    {
        Vector2 dirToTarget = (Player_Trans.position - transform.position).normalized; 

        GameObject bulletObject = Instantiate(Bullet_prefab, transform.position, Quaternion.identity);
        R_Bullet bulletComponent = bulletObject.GetComponent<R_Bullet>();


        if(bulletComponent != null)
        {
            bulletComponent.Init(dirToTarget,transform.position);
        }

    }
    #region 장애물 및 낭떠러지 체크
    public bool CheckForObstacle(Researcher researcher)
    {
        Vector2 checkDirection = (Movedistance > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(researcher.transform.position, checkDirection, wallCheckDistance, researcher.wallLayer);

        return hit.collider != null;
    }

    public bool CheckForLedge(Researcher researcher)
    {

        Vector3 footPosition = researcher.transform.position;
        footPosition.x += Movedistance * 0.3f;

        RaycastHit2D hit = Physics2D.Raycast(footPosition, Vector2.down, groundCheckDistance, researcher.groundLayer);

        return hit.collider == null;
    }
    #endregion

    #region 드론 소환 애니메이션 딜레이용
    public void StopResearcherTimer()
    {
       

        if (Time.time >= statetime)
        {
            ChangeState(R_States[2]);
        }
    }
    #endregion

    // 현재 바라보는 방향 벡터 반환
    public Vector2 GetcurrentVect2()
    {
        float directonx = Mathf.Sign(transform.localScale.x);
        return new Vector2(directonx, 0);
    }
    public void FlipResearcher(Researcher researcher, float direction)
    {

        Vector3 currentScale = researcher.transform.localScale;


        if (direction > 0 && currentScale.x < 0)
        {
            researcher.transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

        else if (direction < 0 && currentScale.x > 0)
        {
            researcher.transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    public void SummonDrone()
    {
        int rand = Random.Range(0, dronespawnpoints.Length);
        Transform spawnPos = dronespawnpoints[rand];
        isDroneSummoned = true;
        GameObject droneObject = Instantiate(D_prefab, spawnPos.position, Quaternion.identity);
        Drone droneComponent = droneObject.GetComponent<Drone>();
        if (droneComponent != null)
        {
            droneComponent.SummonInit(this.transform, Player_Trans);
        }
    }
       
    public void R_Hit()
    {
        
    }
}
