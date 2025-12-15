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
    [SerializeField] private float forwardPower = 6f;  // 앞 방향 힘
    [SerializeField] private float upwardPower = 10f;  // 위 방향 힘

    [Header("거리 기반 힘 보정값")]
    [SerializeField] private float distancePower = 5f;        // 거리값 조절
    [SerializeField] private float minPower = 0.8f;
    [SerializeField] private float maxPower = 1.8f;

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
        float playerPos = boss.player.position.x - boss.transform.position.x;
        int throwDir = playerPos >= 0 ? 1 : -1;

        float bossWidth = boss.GetComponent<Collider2D>().bounds.extents.x;

        Vector3 throwPos = boss.transform.position + new Vector3(throwDir * (bossWidth + 0.3f), 1.2f, 0f);

        GameObject debrisObj = Instantiate(DebrisPrefab, throwPos, Quaternion.identity);
        DebrisProjectile debrisProjectile = debrisObj.GetComponent<DebrisProjectile>();

        Physics2D.IgnoreCollision(
            debrisProjectile.GetComponent<Collider2D>(),
            boss.GetComponent<Collider2D>()
        );

        Vector2 dir = (boss.player.position - throwPos).normalized;

        // y 조금 아래로 눌러서 퍼올리는 느낌 유지
        dir.y = -0.3f;
        dir.Normalize();

        float distance = Mathf.Abs(boss.player.position.x - boss.transform.position.x);
        float distanceFactor = Mathf.Clamp(distance / distancePower, minPower, maxPower);

        float finalForward = forwardPower * distanceFactor;
        float finalUpward = upwardPower * (distanceFactor * 0.5f); 

        debrisProjectile.Launch(dir, finalForward, finalUpward);


        debrisProjectile.Launch(dir, finalForward, finalUpward);
    }



}
