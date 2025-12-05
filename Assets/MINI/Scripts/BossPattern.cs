using UnityEngine;

public abstract class BossPattern : ScriptableObject
{
    protected BossAI boss;
    protected Rigidbody2D rb;
    protected Animator animator;

    public virtual void Initialize(BossAI boss)
    {
        this.boss = boss;
        this.animator = boss.animator;
        this.rb = boss.GetComponent<Rigidbody2D>();
    }   

    public abstract void StartPattern();           // 패턴 시작 할 때 초기화나 애니매이션 재생하는 곳으로 쓰셈
    public abstract void UpdatePattern();          // 패턴 진행 중일 때 필요한 로직 여따 짜셈
    public abstract void ExitPattern();            // 패턴 종료할 때 꺼야 하는 거 용도로 쓰셈 히트박스 같은 거
    public abstract void OnAnimationEvent(string eventName); // 애니메이션 이벤트 받을 때 쓰셈
}
