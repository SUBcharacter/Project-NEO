using UnityEngine;
using System.Collections.Generic;

public class TutoBossAI : BossAI
{
    [Header("튜토보스 패턴 리스트")]
    public List<BossPattern> tutorialSequence = new();  // 순서대로 보여줄 패턴들
    public List<BossPattern> randomPatterns = new();    // 이후 랜덤 패턴들

    private TutoBossState currentState;        // 보스 상태    

    // 추후 수정할 예정 
    [Header("튜토보스 공격 간격")]
    public float lastAttackTime = -999f;    // 패턴 종료 후 바로 움직여버리는 문제 임시 조치 (추후 로직 보완 예정)
    public float minAttackInterval = 2f;    // 공격 텀 확보용 기본쿨 (정확한 값 조정 예정)

    [HideInInspector]
    public int tutorialIndex = 0;                        // 현재 튜토 패턴 단계

    private void Start()
    {
        Debug.Log("Idle 진입 시도");
        // 튜토보스는 Phase 없음 → Idle 상태로 바로 시작
        ChangeState(new TutoIdleState(this));

        
    }
}
