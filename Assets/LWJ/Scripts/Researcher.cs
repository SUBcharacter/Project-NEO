using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
public class Researcher : MonoBehaviour
{
    [SerializeField] public Transform[] dronespawnpoints;
    [SerializeField] public Transform Player_Trans;
    [SerializeField] public GameObject Bullet_prefab;
    public GameObject D_prefab;
    public ResearcherState[] R_States = new ResearcherState[3];
    public ResearcherState currentStates;
    SpriteRenderer spriteRenderer;
    public SightRange sightRange;
    public bool isDroneSummoned = false;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

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

    public void FlipSprite(Vector2 direction)
    {
        if (direction.x > 0)
        {
            spriteRenderer.flipY = false;
        }
        else
        {
            spriteRenderer.flipY = true;

        }
    }
    
    public void ShootBullet(Vector2 dir)
    {
        Vector2 dirToTarget = (Player_Trans.position - transform.position).normalized; 

        // ÃÑ¾Ë »ý¼º
        GameObject bulletObject = Instantiate(Bullet_prefab, transform.position, Quaternion.identity);
        R_Bullet bulletComponent = bulletObject.GetComponent<R_Bullet>();

        if(bulletComponent != null)
        {
            bulletComponent.Init(dirToTarget,10f);
        }



    }

    public void WaitDronetimer()
    {
        StartCoroutine(Waittimerdrone());
    }

    IEnumerator Waittimerdrone()
    {
        yield return CoroutineCasher.Wait(2f);
        ChangeState(R_States[2]);
    }

    public Vector2 GetcurrentVect2()
    {
        float directonx = Mathf.Sign(transform.localScale.x);
        return new Vector2(directonx, 0);
    }

}
