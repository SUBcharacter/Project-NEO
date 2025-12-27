using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Rendering;

public class BossAI : MonoBehaviour, IDamageable
{
    [Header("���� ����")]
    [SerializeField] public Transform player;
    public Animator animator;
    public Rigidbody2D rb;
    public GameObject tackleHitbox;

    [Header("State Info")]
    [SerializeField] protected BossPhase currentPhase;        // ���� ���� ������
    [SerializeField] protected List<BossPhase> allPhases = new();
    private int phaseIndex = 0;


    [Header("Status")]
    [SerializeField] protected float maxHp = 10000f;          // �ӽ� ü��
    [SerializeField] protected float currentHp;
    [SerializeField] protected float currentPoise;            // ���ε�
    protected bool isGroggy = false;


    [SerializeField]private BossState currentState;        // ���� ����    
    protected CancellationTokenSource _cts;                // �񵿱� �۾� ��ҿ�?��ū
    private CancellationTokenSource _patternCts;           // [�߰�] ���� �ߴܿ� 12/25

    #region Ʃ�亸����..
    [Header("TutoBoss")]
    public bool Attacking = false;      // ���� ������ ����

    public BossPattern CurrentPattern { get; private set; }

    // �� �κ��� �ʿ� ����
    // BossIdleState�� ���Խÿ� ������ �̸�(string)���� �б⸦ ������
    // ������ �̸��� "TutoBossPhase"�̸� �ڵ����� TutoIdleBattleState�� �Ѿ. �ڼ��Ѱ�  BossState ����

    #endregion





    // ĸ��ȭ
    public BossPhase CurrentPhase => currentPhase;
    public List<BossPhase> AllPhase => allPhases;
    public CancellationToken DestroyCancellationToken => _cts != null ? _cts.Token : CancellationToken.None;
    public CancellationToken PatternCancellationToken // ������Ƽ
    {
        get
        {
            if (_patternCts != null) { _patternCts = new CancellationTokenSource(); }
            return _patternCts.Token;
        }
    }
    [HideInInspector] public List<GameObject> activeLightWaves = new();   // �ھ� ����Ʈ

    private bool isCoreActive = false;


    private void Awake()
    {
        _cts = new CancellationTokenSource();
        _patternCts = new CancellationTokenSource();

        if (!animator) animator = GetComponent<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHp = maxHp;
        if (allPhases.Count > 0) SetPhase(0);
        else Debug.LogError("BossAI: Phase�̼�������");
        ChangeState(new BossIdleState(this));

        // Ʃ�丮�� ���� �б����Ƕ����� �̷��� �߽��ϴ�.. 
        // �� �κ��� �ʿ� ����
        // BossIdleState�� ���Խÿ� ������ �̸�(string)���� �б⸦ ������
        // ������ �̸��� "TutoBossPhase"�̸� �ڵ����� TutoIdleBattleState�� �Ѿ. �ڼ��Ѱ�  BossState ����
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
    public BossPattern SelectBestPattern() // ���� ���� �˰�����
    {
        if (currentPhase == null) return null;

        List<BossPattern> candidates = currentPhase.availablePatterns;  // �ĺ�
        float totalWeight = 0f;                                         // ����ġ ����

        // ����ġ �ջ�
        foreach (var p in candidates)
        {
            float score = p.EvaluateScore(this);
            if (score > 0) totalWeight += score;
        }

        // ���?�ص� ����? => null
        if (totalWeight <= 0) return null;

        // �귿 ���?��í ���� ����ġ�� �������� Ȯ�� UP
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
        if (isGroggy)
        {
            damage *= 1.5f;
        }

        if (isCoreActive)
        {
            float sum = 0f;
            foreach (var core in activeLightWaves)
            {
                if (core != null)
                {                    
                    sum += core.GetComponent<BossSubCore>().EnhancingAmount;
                }
            }
            currentHp = Mathf.Max(0, currentHp - (damage * sum));
            TakeGroggyDamage(3f);
        }
        else
        {
            currentHp = Mathf.Max(0, currentHp - damage);
            TakeGroggyDamage(3f);
        }
        CheckPhaseTransition();
    }
    public void TakeGroggyDamage(float damage)          // �ھ Ȱ��ȭ�� �Ǿ����� �� �������� �氨�� �Ǵ� ����
    {
        if (!isGroggy)
        {
            currentPoise -= damage;
            if (currentPoise <= 0)
            {
                StartGroggy();
            }
        }
    }
    void StartGroggy()
    {
        isGroggy = true;
        // �׷α� ���·� ��ȯ 
        ChangeState(new GroggyState(this, 3.0f));
    }
    public void ChangeState(BossState newState)
    {
        StopCurrentPattern(); // ��ū ���� 

        currentState?.Exit();
        currentState = newState;
        currentState.Start();
    }
    public void RecoverPoise()
    {
        isGroggy = false;
        currentPoise = currentPhase.maxPoise;
    }
    void CheckPhaseTransition()     // ������ üũ �Լ�
    {
        float ratio = currentHp / maxHp;

        float nextRatio = currentPhase.nextRatio;

        if (ratio <= nextRatio)
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
        currentPoise = currentPhase.maxPoise; // ������ �ٲ� �� ���ε� ����

        Debug.Log($"Phase : {currentPhase.phaseName}");

        // ������ ���� ������ �ִٸ� ���?���� (���?����)
        if (currentPhase.entryPattern != null)
        {
            // ������ AttackingState�� ��ȯ�ϸ� ���?���� ����
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
    private void OnDestroy()
    {
        StopCurrentPattern();
        _cts?.Cancel();
        _cts?.Dispose();
    }
    private void PhaseTestDamage()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(1000f);
            Debug.Log($"Test Damage Current HP: {currentHp}");
        }
    }



    
    #region Ʃ�亸���� �Լ���
    public void SetCurrentPattern(BossPattern pattern)
    {
        CurrentPattern = pattern;
    }

    // ���ϸ��� �Ÿ��� �ٸ��ϱ� Sway�� Dash�� �Ÿ��˻縦 ���� ������µ�?... ����ּ���?
    // �����ϰ� ���� �� �ʿ� ���� �ܼ��ϰ� �÷��̾��?�������� x�� �Ÿ��� ���ؼ� float�� ��ȯ�ϴ� �Լ��� �����?��
    // �� �Լ��� ȣ�� �� ���� �� ������ ���۵Ǵ� Execute�� �ʹ� �κ���.

    public float DistanceToPlayer()
    {
        // x�ุ ���?
        return Mathf.Abs(player.position.x - transform.position.x);
    }
 
    #endregion

    

    public void StopCurrentPattern() // ������Ʈ ���� �� ���� �������� ��Ű�� �Լ�
    {
        if (_patternCts != null)
        {
            _patternCts.Cancel();
            _patternCts.Dispose();
        }
        _patternCts = new CancellationTokenSource();
    }

}
