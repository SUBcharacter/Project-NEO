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

    public ResearcherState[] R_States = new ResearcherState[4];
    public ResearcherState currentStates;

    public SightRange sightRange;

    public bool isDroneSummoned = false;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer flashrender;
    public float R_Speed = 2f;
    public float Movedistance = 1f;
    private float wallCheckDistance = 0.8f;
    private float groundCheckDistance = 0.8f;
    public float WaitTimer = 3f;
    public float statetime;
    private float M_direction;
    [SerializeField] private float currenthealth = 100f;
    [SerializeField] private float knockBackXForce = 0.5f;

    [SerializeField] private Color flashColor = Color.red; 
    [SerializeField] private float flashDuration = 0.1f;    
    [SerializeField] private float invincibilityDuration = 0.5f;
    private Coroutine flashCoroutine;
    private bool isInvincible = false;
    public Rigidbody2D rb;

    void Awake()
    {
        sightRange = GetComponent<SightRange>();    
        spriteRenderer = GetComponent<SpriteRenderer>();
        flashrender = GetComponent<SpriteRenderer>();   
        R_States[0] = new R_IdleState();
        R_States[1] = new R_SummonDroneState();
        R_States[2] = new R_Attackstate();
        R_States[3] = new R_Hitstate();
        ChangeState(R_States[0]);
        rb = GetComponent<Rigidbody2D>();
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
    
    public void PatrolMove()
    {
        M_direction = Mathf.Sign(transform.localScale.x);
        float velocityX = M_direction * R_Speed;
        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);    
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

    public void MovetoPlayer()
    {
        float directionToPlayer = Player_Trans.position.x - transform.position.x;

        float M_direction = Mathf.Sign(directionToPlayer);

        float velocityX = M_direction * R_Speed;

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        FlipResearcher(this, directionToPlayer);

    }
    #region 벽과 낭떠러지 체크
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

    #region 소환 모션 딜레이
    public void StopResearcherTimer()
    {

        if (Time.time >= statetime)
        {
            ChangeState(R_States[2]);
        }
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
       
    public void TakeDamage(float damage)
    {
        Debug.Log("Researcher�� " + damage + "��ŭ�� ���ظ� �Ծ����ϴ�.");
        currenthealth -= damage;

       
        ChangeState(R_States[3]);
        flashCoroutine = StartCoroutine(FlashCoroutin());
        if (currenthealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void Knockback()
    {
        Vector2 knockbackDirection = (Vector2)transform.position - (Vector2)Player_Trans.position;
        float xDirection = Mathf.Sign(knockbackDirection.x);
        Vector2 knockbackForce = new Vector2(xDirection * knockBackXForce, 0f); 
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator FlashCoroutin()
    {
        isInvincible = true;
        Color originalColor = flashrender.color;

        float endTime = Time.time + invincibilityDuration;

        while (Time.time < endTime)
        {
    
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }


        flashrender.color = originalColor;
        isInvincible = false;
        flashCoroutine = null;
    }
}
