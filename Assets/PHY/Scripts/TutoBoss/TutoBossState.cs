using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;

public class TutoIdleBattleState : BossIdleState
{
    public TutoIdleBattleState(BossAI boss) : base(boss)
    {
    }

    public override async void Start()
    {

    }

    public override void Update() { }

    public override void Exit() { }
}

public class TutoSwayState : BossState
{

    public TutoSwayState(BossAI boss) : base(boss)
    {
    }
    public override void Start()
    {
      
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
       
    }


}

public class TutoDashState : BossState
{
    public TutoDashState(BossAI boss) : base(boss)
    {
    }
    public override void Start()
    {
      
    }
    public override void Update()
    {
        
    }

    public override void Exit()
    {
        
    }


}

/// <summary>
/// 어태킹이랑 쿨다운은 모르겠음 일단 가져옴
/// </summary>
public class TutoAttackingState : AttackingState
{
    public TutoAttackingState(BossAI boss, BossPattern pattern = null) : base(boss, pattern)
    {
    }
}

public class TutoCoolDownState : BossCoolDownState
{
    public TutoCoolDownState(BossAI boss) : base(boss)
    {
    }
}
