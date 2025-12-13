using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BossPattern : ScriptableObject
{
    protected BossAI boss;
    protected Rigidbody2D rb;
    protected Animator animator;

    [Header("AI Condition (스마트 선택 조건)")]
    [Tooltip("이 거리보다 가까우면 사용 안 함")]
    public float minRange = 0f;
    [Tooltip("이 거리보다 멀면 사용 안 함")]
    public float maxRange = 20f;
    [Tooltip("재사용 대기시간")]
    public float cooldownTime = 3.0f;
    [Tooltip("가중치 (높을수록 자주 사용)")]
    public float priorityWeight = 1.0f;

    [Header("Combo System (연계기)")]
    [Tooltip("이 패턴 후 바로 시전할 수 있는 패턴들")]
    public List<BossPattern> comboPatterns;
    [Range(0f, 1f)] public float comboChance = 0.5f;

    // 런타임 데이터 (쿨타임 체크용)
    [System.NonSerialized] public float lastUsedTime = -999f;

    public virtual void Initialize(BossAI boss)
    {
        this.boss = boss;
        this.animator = boss.animator;
        this.rb = boss.GetComponent<Rigidbody2D>();
    }  
    
    public float EvaluateScore(BossAI boss)
    {
        // 쿨타임 체크 후
        if (Time.time < lastUsedTime + cooldownTime) return 0f;

        // 사거리 체크 후
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        if (distance < minRange || distance > maxRange) return 0f;

        float finalScore = priorityWeight;

        // 거리 기반 가중치 계산
        // (중간 거리에 가까울수록 높아지게)
        float midRange = (minRange + maxRange) / 2f;
        float distDiff = Mathf.Abs(distance - midRange);

        if (distDiff < 2.0f) // 가까울수록 가중치 증가
            finalScore *= 1.5f; 

        // 가중치 설정
        return finalScore;
    }

    public virtual async Task StartPattern()
    {
        lastUsedTime = Time.time;

        await Execute();
    }
    protected abstract Awaitable Execute();
    public abstract void UpdatePattern();          // 패턴 진행 중일 때 필요한 로직 여따 짜셈
    public abstract void ExitPattern();            // 패턴 종료할 때 꺼야 하는 거 용도로 쓰셈 히트박스 같은 거
    public abstract void OnAnimationEvent(string eventName); // 애니메이션 이벤트 받을 때 쓰셈
}
