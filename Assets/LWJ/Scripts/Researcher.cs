using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
public class Researcher : Enemy
{
    [SerializeField] public Transform[] dronespawnpoints;
    [SerializeField] public Transform Player_Trans;
    [SerializeField] public Transform arm;
    [SerializeField] public Transform Gunfire;

    [SerializeField] public GameObject Bullet_prefab;
    [SerializeField] public GameObject D_prefab;

    public ResearcherState[] R_States = new ResearcherState[7];
    public ResearcherState currentStates;

    public SightRange sightRange;
    public AimRange aimRange;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private SpriteRenderer flashrender;

    private float knockBackXForce = 0.5f;
    private float flashDuration = 0.1f;    
    private float invincibilityDuration = 0.5f;
    private float wallCheckDistance = 1f;
    private float groundCheckDistance = 1f;
    private float M_direction;
    public float nextFireTime;
    public float AimRotationSpeed = 5f;
    private Coroutine flashCoroutine;

    public bool isDroneSummoned = false;
    public bool isarmlock = false;

    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] public Animator animator;
    [SerializeField] private Animator Armanima;

    protected override void Awake()
    {
        base.Awake();
        Transform ch_Trans = transform.Find("Armposition");
       
       if (ch_Trans != null)
       {
           Transform arm_trans = ch_Trans.Find("ArmSprite");
       
           if (arm_trans != null)
           {
               Armanima = arm_trans.GetComponent<Animator>();
           }
           else
           {
               Debug.LogError("ArmSprite를 'Armposition' 자식에서 찾을 수 없습니다.");
           }
       }
       else
       {
           Debug.LogError("Armposition 자식 오브젝트를 찾을 수 없습니다.");
       }
      
        sightRange = GetComponent<SightRange>();   
        aimRange = GetComponent<AimRange>();
        flashrender = GetComponent<SpriteRenderer>();   
        R_States[0] = new R_IdleState();
        R_States[1] = new R_WalkState();
        R_States[2] = new R_SummonDroneState();
        R_States[3] = new R_Attackstate();
        R_States[4] = new R_Hitstate();
        R_States[5] = new R_ChaseState();
        R_States[6] = new R_Deadstate();
        ChangeState(R_States[0]);
        animator = GetComponent<Animator>();
        arm = transform.Find("Armposition");
        Gunfire = transform.Find("GunTip");

       
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
    
    public override void Move()
    {
        M_direction = Mathf.Sign(transform.localScale.x);
        float velocityX = M_direction * enemyData.moveSpeed;
        rigid.linearVelocity = new Vector2(velocityX, rigid.linearVelocity.y);
    }

    public override void Chase()
    {
        float directionToPlayer = Player_Trans.position.x - transform.position.x;

        float M_direction = Mathf.Sign(directionToPlayer);

        float velocityX = M_direction * enemyData.moveSpeed;

        rigid.linearVelocity = new Vector2(velocityX, rigid.linearVelocity.y);

        FlipResearcher(this, directionToPlayer);
        FlipArm(directionToPlayer); 

    }

    #region 애니메이션 키 이벤트 함수
    public void R_die()
    {
      Die();
    }
    
    public void summontoattack()
    {
        ChangeState(R_States[5]);
    }
    #endregion

    #region 벽과 낭떠러지 체크
    public bool CheckForObstacle(Researcher researcher)
    {
        float direction = Mathf.Sign(researcher.transform.localScale.x);
        Vector2 checkDirection = (direction > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(researcher.transform.position, checkDirection, wallCheckDistance, researcher.wallLayer);

        return hit.collider != null;
    }

    public bool CheckForLedge(Researcher researcher)
    {
        float direction = Mathf.Sign(researcher.transform.localScale.x);
        Vector3 footPosition = researcher.transform.position;
        footPosition.x += direction * 0.3f;

        RaycastHit2D hit = Physics2D.Raycast(footPosition, Vector2.down, groundCheckDistance, researcher.groundLayer);

        return hit.collider == null;
    }
    #endregion

    #region 팔 관련 함수
    public void Armsetactive(bool isactive)
    {
        arm.gameObject.SetActive(isactive);

        if (isactive)
        {
            arm.rotation = Quaternion.Euler(0, 0, 0);

            float currentDirection = transform.localScale.x;

            Vector3 armLocalScale = arm.localScale;

            if (currentDirection < 0) 
            {
                armLocalScale.x = -1;
                armLocalScale.y = -1;
            }
            else 
            {
                armLocalScale.x = 1;
                armLocalScale.y = 1;
            }

            arm.localScale = armLocalScale;

            if (!isarmlock)
            {
                Aimatplayer(true); 
            }

        }
    }
    public void Aimatplayer(bool active = false)
    {
        if (isarmlock) return;

        Vector3 dir = Player_Trans.position - arm.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        if (active) 
        {
            arm.rotation = targetRotation;
        }
        else 
        {
            arm.rotation = Quaternion.Slerp(arm.rotation,targetRotation,Time.deltaTime * AimRotationSpeed);
        }
    }

    public void FlipArm(float direction)
    {

        Vector3 armLocalScale = arm.localScale;

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

    public void ShootBullet()
    {
        Vector3 startPosition = Gunfire != null ? Gunfire.position : arm.position;
        Vector2 dirToTarget = arm.right;

        GameObject bulletObject = Instantiate(Bullet_prefab, startPosition, Quaternion.identity);
        R_Bullet bulletComponent = bulletObject.GetComponent<R_Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.Init(dirToTarget, startPosition);
        }
        Debug.Log("발사");
    }
    public override void Attack()
    {

        if (aimRange != null && aimRange.IsPlayerInSight)
        {
            Aimatplayer();
            if (Time.time >= nextFireTime)
            {

                PlayShot();
                nextFireTime = Time.time + enemyData.fireCooldown;
            }
        }
        else
        {
            Debug.Log("사격 범위 이탈, 시야 유지. 추적으로 전환.");
            ChangeState(R_States[5]);
        }
    }

    public void PlayShot()
    {
        isarmlock = true;
        Armanima.Play("R_Shot");
    }

    public void Armshotend()
    {
        isarmlock = false;
    }
    #endregion

    public void WallorLedgeFlip(Researcher researcher)
    {
        if (CheckForObstacle(researcher) || CheckForLedge(researcher))
        {
            enemyData.moveDistance *= -1;
            researcher.FlipResearcher(researcher, enemyData.moveDistance);
            return;
        }
        else
        {
            Move();
        }
    }
    public void FlipResearcher(Researcher researcher, float direction)
    {
        if (isarmlock) return;

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
       
    public override void TakeDamage(float damage)
    {
        currnetHealth -= damage;
        Debug.Log("체력 : " + currnetHealth);

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            spriteRenderer.color = Color.white;
        }

        if (currnetHealth <= 0)
        {
            ChangeState(R_States[6]);
        }
        else
        {
            ChangeState(R_States[4]);
            flashCoroutine = StartCoroutine(FlashCoroutin());
        }
    }

    public void Knockback()
    {
        Vector2 knockbackDirection = (Vector2)transform.position - (Vector2)Player_Trans.position;
        float xDirection = Mathf.Sign(knockbackDirection.x);
        Vector2 knockbackForce = new Vector2(xDirection * knockBackXForce, 0f);
        rigid.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator FlashCoroutin()
    {

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
        flashCoroutine = null;
    }
}
