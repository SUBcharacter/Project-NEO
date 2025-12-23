using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Rendering;

public class BossAI : MonoBehaviour, IDamageable
{
    [Header("참조 변수")]
    [SerializeField] public Transform player;
    public Animator animator;
    public Rigidbody2D rb;
    public GameObject tackleHitbox;

    [Header("State Info")]
    [SerializeField] protected BossPhase currentPhase;        // 보스 현재 페이즈
    [SerializeField] protected List<BossPhase> allPhases = new();
    private int phaseIndex = 0;


    [Header("Status")]
    [SerializeField] protected float maxHp = 10000f;          // 임시 체력
    [SerializeField] protected float currentHp;
    [SerializeField] protected float currentPoise;            // 강인도
    protected bool isGroggy = false;

    [SerializeField]private BossState currentState;        // 보스 상태    
    protected CancellationTokenSource _cts;                // 비동기 작업 취소용 토큰

    // 2025-12-21 효영 추가
    #region 튜토보스용..
    [Header("TutoBoss")]
    public bool Attacking = false;      // 공격 중인지 여부

    public BossPattern CurrentPattern { get; private set; }

    // 이 부분은 필요 없음
    // BossIdleState로 진입시에 페이즈 이름(string)으로 분기를 나눠둠
    // 페이즈 이름이 "TutoBossPhase"이면 자동으로 TutoIdleBattleState로 넘어감. 자세한건  BossState 참고

    #endregion


    // 캡슐화
    public BossPhase CurrentPhase => currentPhase;
    public List<BossPhase> AllPhase => allPhases;
    public CancellationToken DestroyCancellationToken => _cts != null ? _cts.Token : CancellationToken.None;    


    [HideInInspector] public List<GameObject> activeLightWaves = new();


    private void Awake()
    {
        _cts = new CancellationTokenSource();
        if(!animator) animator = GetComponent<Animator>();
        if(!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHp = maxHp;
        if (allPhases.Count > 0) SetPhase(0);
        else Debug.LogError("BossAI: Phase미설정상태");
        ChangeState(new BossIdleState(this));

        // 튜토리얼 보스 분기조건때문에 이렇게 했습니다.. 
        // 이 부분은 필요 없음
        // BossIdleState로 진입시에 페이즈 이름(string)으로 분기를 나눠둠
        // 페이즈 이름이 "TutoBossPhase"이면 자동으로 TutoIdleBattleState로 넘어감. 자세한건  BossState 참고
        //if (isTutorialBoss)
        //    ChangeState(new TutoIdleBattleState(this));
        //else
        //    ChangeState(new BossIdleState(this));
        //currentState.Star t();
    }
    void Update()
    {
        currentState?.Update();
        
        PhaseTestDamage();
    }
    public BossPattern SelectBestPattern() // 패턴 선택 알고리즘
    {        
        if (currentPhase == null) return null; 

        List<BossPattern> candidates = currentPhase.availablePatterns;  // 후보
        float totalWeight = 0f;                                         // 가중치 변수

        // 가중치 합산
        foreach (var p in candidates)
        {            
            float score = p.EvaluateScore(this);
            if (score > 0) totalWeight += score;
        }

        // 계산 해도 없다? => null
        if (totalWeight <= 0) return null;

        // 룰렛 방식 가챠 패턴 가중치가 높을수록 확률 UP
        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        foreach (var p in candidates)
        {
            float score = p.EvaluateScore(this);
            if (score > 0)
            {
                currentSum += score;
                if (randomValue <= currentSum) return p;
            }
        }

        return null;
    }
    public void TakeDamage(float damage)
    {
        if (isGroggy) damage *= 1.5f; // 그로기 때는 더 아프게

        currentHp = Mathf.Max(0, currentHp - damage);

        // 강인도 깎기 (데미지에 비례하거나 고정값) <- 어째해야할지 논의해야 함
        if (!isGroggy)
        {
            currentPoise -= damage; // 여기다 변수하나 해서 넣어보기..정도? ex) 근접이면 1.5,원거리는 1.0? 이런느낌
            if (currentPoise <= 0)
            {
                StartGroggy();
            }
        }
    
        CheckPhaseTransition();
    }
    void StartGroggy()
    {
        isGroggy = true;
        // 그로기 상태로 전환 
        ChangeState(new GroggyState(this, 3.0f));
    }
    public void ChangeState(BossState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Start();
    }
    public void RecoverPoise()
    {
        isGroggy = false;
        currentPoise = currentPhase.maxPoise;
    }
    void CheckPhaseTransition()     // 페이즈 체크 함수
    {
        float ratio = currentHp / maxHp;     
 
        float nextRatio = currentPhase.nextRatio;
        
        if (ratio <=nextRatio)
        {
            phaseIndex++;
            SetPhase(phaseIndex);
        }
    }
    public void SetPhase(int index)
    {
        if (index >= allPhases.Count) return;

        currentPhase = allPhases[index];
        animator.speed = currentPhase.speedMultiplier;
        currentPoise = currentPhase.maxPoise; // 페이즈 바뀔 때 강인도 리셋
        
        Debug.Log($"Phase : {currentPhase.phaseName}");

        // 페이즈 진입 패턴이 있다면 즉시 실행 (기믹 패턴)
        if (currentPhase.entryPattern != null)
        {
            // 강제로 AttackingState로 전환하며 기믹 패턴 주입
            ChangeState(new AttackingState(this, currentPhase.entryPattern));
        }
    }
    public void OnAnimationTrigger(string eventName) => currentState?.OnAnimationEvent(eventName);
    public void FaceTarget(Vector3 targetPos)
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        if (targetPos.x > transform.position.x) transform.localScale = new Vector3(scaleX, transform.localScale.y, 1);
        else transform.localScale = new Vector3(-scaleX, transform.localScale.y, 1);
    }
    private void OnDestroy() { _cts?.Cancel(); _cts?.Dispose(); }
    private void PhaseTestDamage()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(1000f);
            Debug.Log($"Test Damage Current HP: {currentHp}");
        }
    }


    
    #region 튜토보스용 함수들
    public void SetCurrentPattern(BossPattern pattern)
    {
        CurrentPattern = pattern;
    }

    // 패턴마다 거리가 다르니까 Sway랑 Dash의 거리검사를 위해 만들었는데.... 살려주세요
    // 복잡하게 생각 할 필요 없이 단순하게 플레이어와 보스간의 x축 거리를 구해서 float로 반환하는 함수를 만들면 됨
    // 이 함수가 호출 될 곳은 각 패턴이 시작되는 Execute의 초반 부분임.

    public float DistanceToPlayer()
    {
        // x축만 사용
        return Mathf.Abs(player.position.x - transform.position.x);
    }
 
    #endregion

}
