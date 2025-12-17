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
    [SerializeField] private float delay = 3.5f;

    private bool isShockTriggerd = false;

    public override async Task StartPattern()
    {
        isShockTriggerd = false;
        await Execute();        // 얘는 또 쓰는군
    }

    protected override async Awaitable Execute()
    {
        boss.FaceTarget(boss.player.position);

        animator.SetTrigger("ElecShock");
        Debug.Log("전기충격 패턴 실행됨");

        while (!isShockTriggerd)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        // 전기충격 후 3-4초 유지
        await Awaitable.WaitForSecondsAsync(delay, boss.DestroyCancellationToken);

        ExitPattern();
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "ShockEvent")
        {
            Debug.Log("전기충격 애니메이션 실행됨");
            ShockWave();
            isShockTriggerd = true;
        }
    }

    public override void UpdatePattern()
    {

    }

    public override void ExitPattern()
    {
        isShockTriggerd = false;
        Debug.Log("전기충격 패턴 종료 -> 추적으로 돌아감");
       
    }
    private void ShockWave()
    {
        Vector3 pos = boss.transform.position + new Vector3(0, shockSpawnY, 0);

        GameObject obj = Instantiate(ShockWavePrefab, pos, Quaternion.identity);

        var hitbox = obj.GetComponent<ElecHitBox>();
        if (hitbox != null)
            hitbox.Init();
    }

}
