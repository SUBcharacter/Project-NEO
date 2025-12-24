using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 왜왜왜 쌰갈 안나오는데 로그도 다 찍히는데 왜 안나오는데 쌰갈
/// </summary>

[CreateAssetMenu(fileName = "BasicPunch", menuName = "TutoBoss/TutoBossPattern/BasicPunch")]
public class BasicPunch : BossPattern
{
    [Header("히트박스 2개 (오른손 / 왼손)")]
    [SerializeField] private GameObject rightPunchHitbox;
    [SerializeField] private GameObject leftPunchHitbox;

    private bool isRightPunchDone = false;
    private bool isLeftPunchDone = false;

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

        // 플레이어 바라보기
        boss.FaceTarget(boss.player.position);

        // 애니메이션 실행
        animator.SetTrigger("Punch");
        Debug.Log("원투펀치 들어옴");

        // 오른손 펀치 기다리기
        while (!isRightPunchDone)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        // 왼손 펀치 기다리기
        while (!isLeftPunchDone)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        // 후딜 (펀치 후 0.3초 정도)
        await Awaitable.WaitForSecondsAsync(0.3f, boss.DestroyCancellationToken);

        ExitPattern();
    }


    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "RightPunchStart")
            rightPunchHitbox.SetActive(true);

        if (eventName == "RightPunchEnd")
        {
            rightPunchHitbox.SetActive(false);
            isRightPunchDone = true;
        }

        if (eventName == "LeftPunchStart")
            leftPunchHitbox.SetActive(true);

        if (eventName == "LeftPunchEnd")
        {
            leftPunchHitbox.SetActive(false);
            isLeftPunchDone = true;
        }
    }

    public override void UpdatePattern()
    {

    }

    public override void ExitPattern()
    {
        // 혹시 남아있으면 끄기
        rightPunchHitbox.SetActive(false);
        leftPunchHitbox.SetActive(false);

        isRightPunchDone = false;
        isLeftPunchDone = false;
    }


}
