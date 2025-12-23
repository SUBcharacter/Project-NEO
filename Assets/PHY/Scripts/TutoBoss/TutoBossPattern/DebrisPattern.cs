using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// [DebrisPattern]
/// - 보스가 잔해를 던지는 패턴
/// - ThrowEvent(애니메이션 이벤트) 발생 시 DebrisProjectile 생성
/// - 플레이어가 공중인지/지상인지에 따라 투척 방식 분기
/// </summary>
/// 
[CreateAssetMenu(fileName = "DebrisPattern", menuName = "TutoBoss/TutoBossPattern/Debris")]
public class DebrisPattern : BossPattern
{
    [Header("더미 오브젝트")]
    [SerializeField] private GameObject DebrisPrefab;

    // SO로 뺄 예정
    [SerializeField] private float throwSpeed = 10f;    // 던지는 속도
    [SerializeField] private float throwX = 6f;        // 보스 기준 얼마나 떨어진 곳으로 던질지
    [SerializeField] private float throwY = 0.5f;      // 던지는 높이 (플레이어 몸통쪽으로)

    private bool isThrow = false;

    protected override async Awaitable Execute()
    {
        // 이곳에서 if 문(또는 다른 조건문)을 통해서 사거리 판단을 진행하면 됨
        // BossAI에 만들어둔 거리 산출 함수를 이용할 것.
        // 최소 사거리보다 거리가 작다면 Sway 상태로 ChangeState 후 return
        // 최대 사거리보다 거리가 크다면 Dash 상태로 ChangeState 후 return
        // 리턴만 잘 실행해 준다면, 패턴이 실행되지 않고, 곧바로 Sway나 Dash로 전환 할 수 있음

        isThrow = false;

        // 1. 보스 방향 조정
        boss.FaceTarget(boss.player.position);

        // 2. Throw 애니메이션 발동
        animator.SetTrigger("Throw");

        // 3. ThrowEvent 올 때까지 대기
        while (!isThrow)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        // 4. 후딜
        await Awaitable.WaitForSecondsAsync(0.3f, boss.DestroyCancellationToken);

        // 5. 패턴 종료
        ExitPattern();
    }


    public override void OnAnimationEvent(string eventName)
    {
        Debug.Log("ThrowEvent 들어옴");

        if (eventName == "ThrowEvent")
        {
            SpawnDebris();
            isThrow = true;

        }
    }

    public override void UpdatePattern() { }
  
    public override void ExitPattern()
    {
        isThrow = false;
        IsFinished = true;
        lastUsedTime = Time.time;
    }


    private void SpawnDebris()
    {

        Vector3 bossPos = boss.transform.position;
        Vector3 playerPos = boss.player.position;

        float distance = Vector2.Distance(playerPos, bossPos);

        float xDir = Mathf.Abs(playerPos.x - bossPos.x);
        float yDir = Mathf.Abs(playerPos.y - bossPos.y);

        bool isGround = xDir >= 6f;

        bool isAir = yDir > 1f;

        // 플레이어 공중에 있을 때 x,y 계산해서 던지기 
        if (isAir && isGround)
        {
            AirDebris(bossPos, playerPos);
            return;
        }

        GroundDebris(bossPos, playerPos);

    }

    // 기존에 지면에서 던지기 
    public void GroundDebris(Vector3 bossPos, Vector3 playerPos)
    {
        Vector2 dir = (playerPos - bossPos);

        // 완전 직선으로 만들기
        dir.y = 0f;
        dir.Normalize();

        float xDir = Mathf.Sign(dir.x);

        Vector3 spawnPos = bossPos + new Vector3(xDir * throwX, throwY, 0f);
        LaunchDebris(dir, spawnPos);

    }

    // 공중에 던지기 (플레이어가 공중에 있을 때 대각선 방향으로)
    public void AirDebris(Vector3 bossPos, Vector3 playerPos)
    {
        Vector2 dir = (playerPos - bossPos).normalized;

        float xDir = Mathf.Sign(dir.x);

        // 보스 몸 바깥으로 스폰 위치 설정
        Vector3 spawnPos = bossPos + new Vector3(xDir * throwX, throwY, 0f);
        LaunchDebris(dir, spawnPos);
    }

    public void LaunchDebris(Vector2 dir, Vector3 spawnPos)
    {
        GameObject Obj = Instantiate(DebrisPrefab, spawnPos, Quaternion.identity);
        DebrisProjectile debrisProjectile = Obj.GetComponent<DebrisProjectile>();

        Physics2D.IgnoreCollision(debrisProjectile.col, boss.GetComponent<Collider2D>());
        debrisProjectile.Launch(dir, throwSpeed);
    }
}
 