//using System.Threading.Tasks;
//using UnityEngine;
///// <summary>
///// 전기충격파 패턴
///// </summary>
//[CreateAssetMenu(fileName = "ElecShockPattern", menuName = "TutoBoss/TutoBossPattern/ElecShock")]
//public class ElecShockPattern : BossPattern
//{
//    [Header("충격파 프리팹")]
//    [SerializeField] private GameObject ShockWavePrefab;

//    [SerializeField] private float shockSpawnY = -0.5f;
//    [SerializeField] private float delay = 3.5f;

//    private bool isShockTriggerd = false;

//    public override async Task StartPattern()
//    {
//        isShockTriggerd = false;
//        await Execute();        // 얘는 또 쓰는군
//    }

//    protected override async Awaitable Execute()
//    {
//        boss.FaceTarget(boss.player.position);

//        animator.SetTrigger("ElecShock");
//        Debug.Log("전기충격 패턴 실행됨");

//        while (!isShockTriggerd)
//            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

//        // 전기충격 후 3-4초 유지
//        await Awaitable.WaitForSecondsAsync(delay, boss.DestroyCancellationToken);

//        ExitPattern();
//    }

//    public override void OnAnimationEvent(string eventName)
//    {
//        if (eventName == "ShockEvent")
//        {
//            Debug.Log("전기충격 애니메이션 실행됨");
//            ShockWave();
//            isShockTriggerd = true;
//        }
//    }

//    public override void UpdatePattern()
//    {

//    }

//    public override void ExitPattern()
//    {
//        isShockTriggerd = false;
//        Debug.Log("전기충격 패턴 종료 -> 추적으로 돌아감");

//    }
//    private void ShockWave()
//    {
//        Vector3 pos = boss.transform.position + new Vector3(0, shockSpawnY, 0);

//        GameObject obj = Instantiate(ShockWavePrefab, pos, Quaternion.identity);

//        var hitbox = obj.GetComponent<ElecHitBox>();
//        if (hitbox != null)
//            hitbox.Init();
//    }

//}

using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 전기충격파 패턴
/// </summary>
[CreateAssetMenu(fileName = "ElecShockPattern", menuName = "TutoBoss/TutoBossPattern/ElecShock")]
public class ElecShockPattern : BossPattern
{
    [Header("충격파 프리팹")]
    [SerializeField] private GameObject ShockWavePrefab;

    [SerializeField] private float shockSpawnY = -0.5f;

    // 전충 유지시간 (3~4초 추천)
    [SerializeField] private float holdTime = 3.5f;

    private bool isShockTriggered = false;

    public override async Task StartPattern()
    {
        isShockTriggered = false;
        await Execute();        // 얘는 또 쓰는군
    }

    protected override async Awaitable Execute()
    {
        boss.FaceTarget(boss.player.position);
        animator.SetTrigger("ElecShock");

        Debug.Log("[전충] 패턴 시작");

        // 애니메이션에서 'ShockEvent'가 들어올 때까지 대기
        while (!isShockTriggered)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        // 전기충격 후 유지시간
        await Awaitable.WaitForSecondsAsync(holdTime, boss.DestroyCancellationToken);

        boss.OnAnimationTrigger("AttackEnd");
        // 패턴 종료 신호
        ExitPattern();

    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "ShockEvent")
        {
            Debug.Log("[전충] ShockEvent 수신 → 충격파 생성");
            ShockWave();
            isShockTriggered = true;
        }
    }

    public override void UpdatePattern() { }

    public override void ExitPattern()
    {
        isShockTriggered = false;
        
        //boss.OnAnimationTrigger("AttackEnd"); // 추적 패턴으로 복귀
        Debug.Log("[전충] ExitPattern 호출됨");
    }

    private void ShockWave()
    {
        Vector3 pos = boss.transform.position + new Vector3(0, shockSpawnY, 0);

        GameObject obj = Instantiate(ShockWavePrefab, pos, Quaternion.identity);

        if (obj.TryGetComponent(out ElecHitBox hitbox))
            hitbox.Init();
    }

   
}

