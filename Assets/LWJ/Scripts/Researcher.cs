using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Researcher : MonoBehaviour
{
    [SerializeField] public Transform[] dronespawnpoints;
    [SerializeField] public Transform Player_Trans;
    public Drone D_prefab;
    public ResearcherState[] R_States = new ResearcherState[3];
    public ResearcherState currentStates;
    SpriteRenderer spriteRenderer;
    public SightRange sightRange;
    public bool isDroneSummoned = false;
    void Awake()
    {
        sightRange = GetComponent<SightRange>();    
        spriteRenderer = GetComponent<SpriteRenderer>();
        R_States[0] = new R_IdleState();
        R_States[1] = new R_SummonDroneState();
        R_States[2] = new R_Attackstate();       
    }

    void Start()
    {
        ChangeState(R_States[0]);
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

}
