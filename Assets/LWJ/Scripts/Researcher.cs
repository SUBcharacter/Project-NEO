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

    public ResearcherState[] R_States = new ResearcherState[5];
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
    public float Idlewaittime;
    [SerializeField] private float currenthealth = 100f;
    [SerializeField] private float knockBackXForce = 0.5f;

    [SerializeField] private Color flashColor = Color.red; 
    [SerializeField] private float flashDuration = 0.1f;    
    [SerializeField] private float invincibilityDuration = 0.5f;
    private Coroutine flashCoroutine;
    private bool isInvincible = false;
    private bool ismove = false;
    private bool isSummon = false;
    private bool isDead = false;
    private bool isAttack = false;
    public Rigidbody2D rb;
    private float originalArmLocalX;
    private float originalArmLocalY;
    private Animator animator;

    void Awake()
    {
        sightRange = GetComponent<SightRange>();    
        spriteRenderer = GetComponent<SpriteRenderer>();
        flashrender = GetComponent<SpriteRenderer>();   
        R_States[0] = new R_IdleState();
        R_States[1] = new R_WalkState();
        R_States[2] = new R_SummonDroneState();
        R_States[3] = new R_Attackstate();
        R_States[4] = new R_Hitstate();
        ChangeState(R_States[0]);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        arm = transform.Find("Armposition");
        Gunfire = transform.Find("GunTip");
        originalArmLocalX = Mathf.Abs(arm.localPosition.x);
        // Y는 그대로 저장합니다.
        originalArmLocalY = arm.localPosition.y;
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

    public void ShootBullet() // 매개변수 (Vector2 dir) 제거
    {
        // 1. 발사 위치는 GunTip (또는 ArmPivot)으로 설정
        Vector3 startPosition = Gunfire != null ? Gunfire.position : arm.position;

        // 2. 발사 방향은 팔의 Forward 방향 (Quaternion.Euler로 계산된 회전 방향)
        Vector2 dirToTarget = (Player_Trans.position - startPosition).normalized;

        GameObject bulletObject = Instantiate(Bullet_prefab, startPosition, Quaternion.identity);
        R_Bullet bulletComponent = bulletObject.GetComponent<R_Bullet>();

        if (bulletComponent != null)
        {
            // 플레이어를 향하는 벡터를 직접 전달
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

        // FlipArm()으로 Y축 반전했기 때문에 회전 적용 방식 구분
        if (arm.localRotation.y < 0) // 왼쪽 보는 중
            arm.rotation = Quaternion.Euler(0, 0, 180 + angle);
        else
            arm.rotation = Quaternion.Euler(0, 0, angle);
    }
    #endregion

    public void FlipArm(float direction)
    {

        Vector3 armLocalScale = arm.localScale;
        Vector3 armLocalPosition = arm.localPosition;

        // 1. 방향에 따른 Y 스케일 및 X 위치 설정
        if (direction < 0) // 왼쪽을 바라볼 때
        {
            armLocalScale.y = -1;
            armLocalPosition.x = -originalArmLocalX; //  X 위치 반전 (왼쪽 어깨 고정)
        }
        else // 오른쪽을 바라볼 때
        {
            armLocalScale.y = 1;
            armLocalPosition.x = originalArmLocalX; //  X 위치 정상화 (오른쪽 어깨 고정)
        }

        // 2. Y 위치를 기본값으로 고정 (Aimatplayer에서 오프셋 적용을 위한 베이스)
        armLocalPosition.y = originalArmLocalY; //  Y 위치 고정

        // 3. 변경된 값 적용
        arm.localScale = armLocalScale;
        arm.localPosition = armLocalPosition;

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
