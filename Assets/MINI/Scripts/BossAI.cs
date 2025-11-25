using UnityEngine;
using System.Collections.Generic;
using Unity.Properties;
using System.Threading;

public class BossAI : MonoBehaviour
{
    [Header("컴포넌트 관련 변수")]
    [SerializeField] private BossState currentState;        // 보스 상태
    [SerializeField] private BossPhase currentPhase;        // 보스 현재 페이즈
    [SerializeField] public Animator animator;
    [SerializeField] private Rigidbody2D rb;

    public BossPhase CurrentPhase => currentPhase;

    [Header("실 사용 변수")]
    [SerializeField] public Transform player;
    [SerializeField] public List<BossPhase> allPhases = new();
    private BossStats bossStats;


    // 비동기 작업 취소용 토큰
    private CancellationTokenSource _cts;
    public CancellationToken DestroyCancellationToken => _cts.Token;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bossStats = GetComponent<BossStats>();
    }
    void Start()
    { 
        InitializePhase();
        SetPhase(0);

        currentState = new BossIdleState(this);
        currentState.Start();
    }


    void Update()
    {
        currentState?.Update();
    }

    //public void TakeDamage(float damage)
    //{
    //    CheckPhaseTransition();
    //
    //}
    //public void CheckPhaseTransition()     // 페이즈 체크 함수
    //{
    //    float hpRatio = bossStats.currentHP / bossStats.maxHP;
    //    if (hpRatio <= 0.1f && currentPhase != allPhases[2]) SetPhase(2);           //10퍼 이하 광폭
    //    else if (hpRatio <= 0.6f && currentPhase != allPhases[1]) SetPhase(1);      //60퍼 이하 2페이즈
    //}
    public void SetPhase(int index)
    {
        currentPhase = allPhases[index];
        animator.speed = currentPhase.speedMultiplier;
    }
    public void ChangeState(BossState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Start();
    }
    public void OnAnimationTrigger(string eventName)
    {
        currentState?.OnAnimationEvent(eventName);
    }
    // 기획 변경 될 시 페이즈 별 패턴 조정을 위한 함수
    void InitializePhase()
    {
        BossPhase p1 = new()
        {
            phaseName = "Phase 1"
        };
        p1.shortPattern.Add(new SmashPattern(this));
        p1.middlePattern.Add(new GrabPattern(this));
        //p1.longPattern.Add(여기다 패턴);
        allPhases.Add(p1);

        BossPhase p2 = new()
        {
            phaseName = "Phase 2"
        };
        //p2.shortPattern.Add(여기다 패턴);        
        //p2.middlePattern.Add(여기다 패턴);
        //p2.longPattern.Add(여기다 패턴);
        allPhases.Add(p2);

        BossPhase p3 = new()
        {
            phaseName = "Berserk",
            speedMultiplier = 1.5f
        };
        //p3.shortPattern.Add(여기다 패턴);
        //p3.middlePattern.Add(여기다 패턴);
        //p3.longPattern.Add(여기다 패턴);
        allPhases.Add(p3);

    }
    private void OnDestroy()
    {
        _cts.Cancel();     
        _cts.Dispose();
        _cts = null;
    }
}
