using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 잔해 더미 패턴
/// </summary>

[CreateAssetMenu(fileName = "DebrisPattern", menuName = "TutoBoss/TutoBossPattern/Debris")]
public class DebrisPattern : BossPattern
{
    [Header("더미 오브젝트")]
    [SerializeField] private GameObject DebrisPrefab;

    // 밑에 변수들 SO로 빼도 될 거같은데..
    [SerializeField] private float throwSpeed = 10f;    // 던지는 속도
    [SerializeField] private float throwX = 6f;        // 보스 기준 얼마나 떨어진 곳으로 던질지
    [SerializeField] private float throwY = 0.5f;      // 던지는 높이 (플레이어 몸통쪽으로)

    private bool isThrow = false;


    public override async Task StartPattern()
    {
        Debug.Log("DebrisPattern StartPattern 실행됨");
        await Execute();
    }

    protected override async Awaitable Execute()
    {
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

    public override void UpdatePattern()
    {
       
    }
    public override void ExitPattern()
    {
        isThrow = false;
    }


    private void SpawnDebris()
    {
        // 플레이어까지의 방향 (초기 계산)
        Vector2 direction = (boss.player.position - boss.transform.position).normalized;

        // 완전 직선으로 만들기 (y축 제거)
        direction.y = 0f;
        direction.Normalize();

        // 보스 기준 좌/우 방향
        float xDir = direction.x >= 0 ? 1f : -1f;

        // 보스 몸 바깥으로 스폰 위치 설정
        Vector3 spawnPos = boss.transform.position + new Vector3(xDir * throwX, throwY, 0f);

        GameObject obj = Instantiate(DebrisPrefab, spawnPos, Quaternion.identity);
        DebrisProjectile projectile = obj.GetComponent<DebrisProjectile>();

        // 보스와 충돌 무시
        Physics2D.IgnoreCollision(projectile.col, boss.GetComponent<Collider2D>());

        // 직선으로 던지기
        projectile.Launch(direction, throwSpeed);
    }

#if UNITY_EDITOR
    public void DrawGizmos(BossAI boss)
    {
        if (boss == null || boss.player == null)
            return;

        // 방향 계산
        Vector2 dir = (boss.player.position - boss.transform.position);
        dir.y = 0f;
        dir.Normalize();

        float xDir = dir.x >= 0 ? 1 : -1;

        // 스폰 포지션 계산
        Vector3 spawnPos = boss.transform.position + new Vector3(xDir * throwX, throwY, 0);

        // --- 기즈모 그리기 ---
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPos, 0.25f);  // 스폰 위치

        Gizmos.color = Color.red;
        Gizmos.DrawLine(spawnPos, spawnPos + (Vector3)(dir * 3f)); // 발사 방향
    }
#endif



}
