using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 왜왜왜 쌰갈 안나오는데 로그도 다 찍히는데 왜 안나오는데 쌰갈
/// </summary>

[CreateAssetMenu(fileName = "BasicPunch", menuName = "TutoBoss/TutoBossPattern/BasicPunch")]
public class BasicPunch : BossPattern
{
    [Header("히트박스")]
    [SerializeField] private PunchHitBox punchHitbox;

    private bool isPunchTriggered = false;

    protected override async Awaitable Execute()
    {

        Debug.Log("기본공격 들어옴");
        float dist = boss.DistanceToPlayer();

        if (dist < minRange)
        {
            boss.ChangeState(new TutoSwayState(boss, this));
            return;
        }

        if (dist > maxRange)
        {
            boss.ChangeState(new TutoDashState(boss, this));
            return;
        }

        IsFinished = false;
        isPunchTriggered = false;

        // 플레이어 바라보기
        boss.FaceTarget(boss.player.position);

        // 애니메이션 실행
        animator.SetTrigger("Punch");

        
        while (!isPunchTriggered)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        await Awaitable.WaitForSecondsAsync(0.2f, boss.DestroyCancellationToken);

        boss.OnAnimationTrigger("AttackEnd");
        ExitPattern();
    }


    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "PunchEvent")
        {
            punchHitbox.Init();
            isPunchTriggered = true;
        }

    }



    public override void UpdatePattern()
    {

    }

    public override void ExitPattern()
    {
        punchHitbox.Disable();
        isPunchTriggered = false;
        IsFinished = true;

    }


}
