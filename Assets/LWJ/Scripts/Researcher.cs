using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
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
            bulletComponent.Init(dirToTarget,10f);
        }

    }

    #region 드론 소환 애니메이션 딜레이용 코루틴
    public void WaitDronetimer()
    {
        StartCoroutine(Waittimerdrone());
    }

    IEnumerator Waittimerdrone()
    {
        yield return CoroutineCasher.Wait(2f);
        ChangeState(R_States[2]);
    }
    #endregion
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


}
