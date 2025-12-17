using UnityEngine;
using System.Collections.Generic;

public class TutoBossAI : BossAI
{
    [Header("튜토보스 패턴 리스트")]
    public List<BossPattern> tutorialSequence = new();  // 순서대로 보여줄 패턴들
    public List<BossPattern> randomPatterns = new();    // 이후 랜덤 패턴들

    private TutoBossState currentState;        // 보스 상태    

    [HideInInspector]
    public int tutorialIndex = 0;                        // 현재 튜토 패턴 단계

    private void Start()
    {
        Debug.Log("Idle 진입 시도");
        // 튜토보스는 Phase 없음 → Idle 상태로 바로 시작
        ChangeState(new TutoIdleState(this));

        
    }
}
