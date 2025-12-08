using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class BossAI : MonoBehaviour, IDamageable
{
    [Header("컴포넌트 관련 변수")]
    [SerializeField] private BossState currentState;        // 보스 상태
    [SerializeField] private BossPhase currentPhase;        // 보스 현재 페이즈
    public Animator animator;
    public Rigidbody2D rb;

    public BossPhase CurrentPhase => currentPhase;

    [Header("실 사용 변수")]
    [SerializeField] public Transform player;
    [SerializeField] public float maxHp = 10000f; // 임시 체력
    [SerializeField] public float currentHp;


    [SerializeField] private List<BossPhase> allPhases = new();

    public List<BossPhase> AllPhases => allPhases;

    [HideInInspector] public List<GameObject> activeLightWaves = new();

    // 비동기 작업 취소용 토큰
    private CancellationTokenSource _cts;
    public CancellationToken DestroyCancellationToken => _cts.Token;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHp = maxHp;
        if (allPhases.Count > 0) SetPhase(0);

        currentState = new BossIdleState(this);
        currentState.Start();
    }


    void Update()
    {
        currentState?.Update();
        PhaseTestDamage();
    }
    public void FaceTarget(Vector3 targetPos)
    {
        // 현재 스케일의 크기(절댓값)를 가져옵니다. (예: 2.0f)
        float scaleX = Mathf.Abs(transform.localScale.x);
        float scaleY = transform.localScale.y;
        float scaleZ = transform.localScale.z;

        if (targetPos.x > transform.position.x)
        {
            // 오른쪽을 봐야 함 (양수)
            transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
        else
        {
            // 왼쪽을 봐야 함 (음수)
            transform.localScale = new Vector3(-scaleX, scaleY, scaleZ);
        }
    }
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        CheckPhaseTransition();

    }
    void CheckPhaseTransition()     // 페이즈 체크 함수
    {
        if (allPhases.Count < 3) return;

        float hpRatio = currentHp / maxHp;
        if (hpRatio <= 0.1f && currentPhase != allPhases[2]) SetPhase(2);           //10퍼 이하 광폭
        else if (hpRatio <= 0.6f && currentPhase != allPhases[1]) SetPhase(1);      //60퍼 이하 2페이즈
    }
    public void SetPhase(int index)
    {
        if (index >= allPhases.Count) return;
        currentPhase = allPhases[index];
        if (animator != null) animator.speed = currentPhase.speedMultiplier;
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
    private void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
    private void PhaseTestDamage()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(1000f);
            Debug.Log($"Test Damage Taken. Current HP: {currentHp}");
        }
    }
    // 기획 변경 될 시 페이즈 별 패턴 조정을 위한 함수

    [System.Serializable]
    public class BossPhase
    {
        public string phaseName;                        // 에디터에서 확인 할 용도   
        public float speedMultiplier = 1.0f;            // 패턴 속도 배율

        public List<BossPattern> shortPattern = new();    // 근거리 패턴
        public List<BossPattern> middlePattern = new();   // 중거리 패턴
        public List<BossPattern> longPattern = new();     // 장거리 패턴

    }
}
