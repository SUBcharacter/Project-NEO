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
}
