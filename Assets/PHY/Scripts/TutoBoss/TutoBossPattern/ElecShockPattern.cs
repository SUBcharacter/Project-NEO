using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 모든 주석은 만든 사람이 이해하기 위한 주석입니다.
/// <summary>

/// <summary>
/// [ElecShockPattern]
/// - 보스가 전기 충격파 생성하는 패턴
/// - 애니메이션 이벤트(ShockEvent) 기반으로 충격파 발생
/// - 이후 일정 시간(holdTime) 유지 후 AttackEnd 처리
/// </summary>
[CreateAssetMenu(fileName = "ElecShockPattern", menuName = "TutoBoss/TutoBossPattern/ElecShock")]
public class ElecShockPattern : BossPattern
{
    [Header("충격파 프리팹")]
    [SerializeField] private GameObject ShockWavePrefab;

    [SerializeField] private float shockSpawnY = -0.5f;
    // 충격파 생성 위치 보정 (보스 발밑 기준)

    [SerializeField] private float holdTime = 3.5f;
    // 충격파 유지 시간 (애니 종료 후 추가 대기 시간)

    private bool isShockTriggered = false;
    // ShockEvent 들어오면 true → 로직 진행

    /// <summary>
    /// 메인 패턴 동작
    /// - ShockEvent 올 때까지 대기
    /// - 충격파 발동 후 일정 시간 유지
    /// - AttackEnd 호출로 종료 신호 전달
    /// </summary>
    protected override async Awaitable Execute()
    {
        IsFinished = false;
        isShockTriggered = false;
        boss.FaceTarget(boss.player.position);
        animator.SetTrigger("ElecShock");
        Debug.Log("[전충] 패턴 시작");

        // 1) ShockEvent 들어올 때까지 대기 (애니 기반)
        while (!isShockTriggered)
            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);

        // 2) 충격파 생성 후 유지 시간
        await Awaitable.WaitForSecondsAsync(holdTime, boss.DestroyCancellationToken);

        // 3) 패턴 종료 → AttackEnd 애니메이션 이벤트와 동일 처리
        boss.OnAnimationTrigger("AttackEnd");

        ExitPattern();
    }

    /// <summary>
    /// ShockEvent 애니메이션 이벤트를 받으면 충격파 생성
    /// </summary>
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
        isShockTriggered = false;  // 다음 실행 대비 초기화
        IsFinished = true;
        Debug.Log("[전충] ExitPattern 호출됨");
    }

    /// <summary>
    /// 충격파 생성 함수
    /// - 보스 발밑 기준 위치에서 ShockWavePrefab 생성
    /// - 생성된 오브젝트가 ElecHitBox를 갖고 있다면 Init() 호출
    /// </summary>
    private void ShockWave()
    {
        Vector3 pos = boss.transform.position + new Vector3(0, shockSpawnY, 0);

        GameObject obj = Instantiate(ShockWavePrefab, pos, Quaternion.identity);

        if (obj.TryGetComponent(out ElecHitBox hitbox))
            hitbox.Init();
    }
}
