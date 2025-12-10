using System.Collections;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
public class Researcher : MonoBehaviour, IDamageable
{
    [SerializeField] public Transform[] dronespawnpoints;
    [SerializeField] public Transform Player_Trans;
    [SerializeField] public Transform arm;
    [SerializeField] public Transform Gunfire;

    [SerializeField] public GameObject Bullet_prefab;
    [SerializeField] public GameObject D_prefab;

    public ResearcherState[] R_States = new ResearcherState[6];
    public ResearcherState currentStates;

    public SightRange sightRange;
    public AimRange aimRange;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer flashrender;

    public float R_Speed = 2f;
    public float Movedistance = 1f;
    public float WaitTimer = 3f;
    public float statetime;
    public float Idlewaittime;
    private float currenthealth = 100f;
    private float knockBackXForce = 0.5f;
    private float flashDuration = 0.1f;    
    private float invincibilityDuration = 0.5f;
    private float wallCheckDistance = 0.8f;
    private float groundCheckDistance = 0.8f;
    private float M_direction;

    private Coroutine flashCoroutine;

    public bool isDroneSummoned = false;
    private bool isInvincible = false;
    private bool ismove = false;
    private bool isSummon = false;
    private bool isDead = false;
    private bool isAttack = false;
    
    public Rigidbody2D rb;

    [SerializeField] private Color flashColor = Color.red;

    private Animator animator;
    private Animator Armanima;
    

    void Awake()
    {
        Transform ch_Trans = transform.Find("Armposition");
        Transform arm_trans = ch_Trans.Find("ArmSprite");
        Armanima = arm_trans.GetComponent<Animator>();
        sightRange = GetComponent<SightRange>();   
        aimRange = GetComponent<AimRange>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        flashrender = GetComponent<SpriteRenderer>();   
        R_States[0] = new R_IdleState();
        R_States[1] = new R_WalkState();
        R_States[2] = new R_SummonDroneState();
        R_States[3] = new R_Attackstate();
        R_States[4] = new R_Hitstate();
        R_States[5] = new R_ChaseState();
        ChangeState(R_States[0]);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        arm = transform.Find("Armposition");
        Gunfire = transform.Find("GunTip");
        Armanima = GetComponentInChildren<Animator>();
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

    public void ShootBullet() 
    {
     
        Vector3 startPosition = Gunfire != null ? Gunfire.position : arm.position;

      
        Vector2 dirToTarget = (Player_Trans.position - startPosition).normalized;

        GameObject bulletObject = Instantiate(Bullet_prefab, startPosition, Quaternion.identity);
        R_Bullet bulletComponent = bulletObject.GetComponent<R_Bullet>();

        if (bulletComponent != null)
        {
 
            bulletComponent.Init(dirToTarget, startPosition);
        }
    }

    public void MovetoPlayer()
    {
        float directionToPlayer = Player_Trans.position.x - transform.position.x;

        float M_direction = Mathf.Sign(directionToPlayer);

        float velocityX = M_direction * R_Speed;

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        FlipResearcher(this, directionToPlayer);
        FlipArm(directionToPlayer); 

    }

    #region 애니메이션 함수
    public void PlayWalk()
    {
        ismove = true;
        animator.SetBool("R_Move", ismove);
    }

    public void StopWalk()
    {
        ismove = false;
        animator.SetBool("R_Move", ismove);
        
    }

    public void PlayDeath()
    {
        isDead = true;
        animator.SetBool("R_Death",isDead);
    }

    public void PlayAttack()
    {
        isAttack = true;
        animator.SetBool("R_Attack", isAttack);
    }
    public void StopAttack()
    {
        isAttack = false;
        animator.SetBool("R_Attack", isAttack);
    }

    public void PlaySummon()
    {
        isSummon = true;
        animator.SetBool("R_Summon",isSummon);    
    }

    public void StopSummon()
    {
        isSummon = false;
        animator.SetBool("R_Summon", isSummon);
    }

    public void PlayShot()
    {
         Armanima.Play("R_Shot");
    }


    public void Playtriggeranima()
    {
        animator.SetTrigger("R_attack");
    }

    public void PlayIdletoattack()
    {
        animator.SetTrigger("R_ItoA");
    }
    #endregion

    public void Idletowalk()
    {
        ChangeState(R_States[1]);
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

    #region 팔 관련 함수

    public void Armsetactive(bool isactive)
    {
        arm.gameObject.SetActive(isactive);
    }
    public void Aimatplayer()
    {
        Vector3 dir = Player_Trans.position - arm.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arm.rotation = Quaternion.Euler(0, 0, angle);
    }
    #endregion

    public void FlipArm(float direction)
    {

        Vector3 armLocalScale = arm.localScale;

        // 1. 방향에 따른 Y 스케일 및 X 위치 설정
        if (direction < 0) // 왼쪽을 바라볼 때
        {

            armLocalScale.x = -1;
            armLocalScale.y = -1;

        }
        else // 오른쪽을 바라볼 때
        {

            armLocalScale.x = 1;
            armLocalScale.y = 1;
   
        }

        arm.localScale = armLocalScale;


    }
    public void summontoattack()
    {
       ChangeState(R_States[3]);
    }

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
        Debug.Log("Researcher " + damage);
        currenthealth -= damage;

       
        ChangeState(R_States[3]);
        flashCoroutine = StartCoroutine(FlashCoroutin());
        if (currenthealth <= 0)
        {
            PlayDeath();
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
