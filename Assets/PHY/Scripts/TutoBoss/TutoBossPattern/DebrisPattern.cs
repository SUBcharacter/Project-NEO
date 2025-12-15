using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 잔해 더미 패턴
/// </summary>

[CreateAssetMenu(fileName = "DebrisPattern", menuName = "TutoBoss/TutoBossPattern/Debris")]
public class DebrisPattern : BossPattern
{
    [SerializeField] private float BtoPDistance = 6;        // 보스랑 플레이어 사이 거리

    [Header("더미 오브젝트")]
    [SerializeField] GameObject DebrisPrefab;
    [SerializeField] private float ThrowSpeed = 10f;

    private bool isThrow = false;

    public override Task StartPattern()
    {
        isThrow = false;

        boss.FaceTarget(boss.player.position);

        animator.SetTrigger("Throw");
        return Task.CompletedTask;
    }

    // 최소 기능 구현을 위한 테스트 (코드 ㅈㄴ더러울 수 있음)
    public override void OnAnimationEvent(string eventName)
    {
        Debug.Log("ThrowEvent 들어옴");
        if(eventName == "ThrowEvent" && !isThrow)
        {
            isThrow = true;

            // 1) 플레이어가 보스 기준 왼/오른 쪽인지 계산
            float xDiff = boss.player.position.x - boss.transform.position.x;
            int dirSign = xDiff >= 0 ? 1 : -1;

            // 2) 보스 기준 투척 위치 (손 근처)
            Vector3 throwPos = boss.transform.position
                + new Vector3(dirSign * 1.2f, 1f, 0f);

            Debug.Log($"boss={boss.transform.position} throwPos={throwPos} player={boss.player.position} dir={throwPos}");


            // 3) Debris 생성
            GameObject debrisObj = Instantiate(DebrisPrefab, throwPos, Quaternion.identity);
            DebrisProjectile debrisProjectile = debrisObj.GetComponent<DebrisProjectile>();

            // 4) Debris 날아갈 방향 (player 방향)
            Vector2 target = boss.player.position;

            debrisProjectile.Launch(target, 3f, 6f);
        }
       
    }
    public override void UpdatePattern()
    {
       
    }
    public override void ExitPattern()
    {
        isThrow = false;
    }



    protected override Awaitable Execute()
    {
        return Awaitable.EndOfFrameAsync();
    }

   
}
