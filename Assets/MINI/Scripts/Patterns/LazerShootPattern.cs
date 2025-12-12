using UnityEngine;

[CreateAssetMenu(fileName = "LazerShootPattern", menuName = "Boss/Patterns/LazerShoot")]
public class LazerShootPattern : BossPattern
{
    [Header("Settings")]
    [SerializeField] private GameObject lazerPrefab;
    [SerializeField] private float maxDistance = 30f;            // 레이저 최대 사거리
    [SerializeField] private float chargeTime = 1.5f;            // 기모으는 시간
    [SerializeField] private float laserDuration = 1.0f;         // 레이저 줄어드는 시간
    [SerializeField] private float laserThickness = 1.0f;        // 레이저 Y축 두께
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0f, 0.5f); // 레이저 높이 보정

    private LayerMask terrainLayer;

    protected override async Awaitable Execute()
    {
        if (boss == null) return;
        if (terrainLayer == 0) terrainLayer = LayerMask.GetMask("Terrain", "Ground");

        // 방향 설정
        boss.FaceTarget(boss.player.position);
        float facingDir = Mathf.Sign(boss.transform.localScale.x);
        Vector2 fireDir = new(facingDir, 0);

        // 전조 (기 모으기)
        boss.animator.SetTrigger("PointToPlayer"); // 혹은 ChargeLazer
        try
        {
            await Awaitable.WaitForSecondsAsync(0.2f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { return; }

        boss.animator.SetTrigger("ChargeLazer");
        try
        {
            await Awaitable.WaitForSecondsAsync(chargeTime, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { return; }

        // 발사
        boss.animator.SetTrigger("FireLazer");

        Vector2 startPos = (Vector2)boss.transform.position + new Vector2(muzzleOffset.x * facingDir, muzzleOffset.y);

        // 벽 감지 
        RaycastHit2D hit = Physics2D.Raycast(startPos, fireDir, maxDistance, terrainLayer);

        float finalDistance = maxDistance;
        if (hit.collider != null)
        {
            finalDistance = hit.distance;
        }

        // 레이저 생성 및 배치 (윤기 공식)       
        Vector2 spawnPos = startPos + (fireDir * (finalDistance * 0.5f));
        GameObject lazer = Instantiate(lazerPrefab, spawnPos, Quaternion.identity);
        lazer.transform.localScale = new Vector3(finalDistance, laserThickness, 1f);

        // 만약 레이저 프리팹이 회전이 필요하다면 방향에 맞춰 회전
        // lazer.transform.right = fireDir; 
        // 하지만 단순히 Box 형태라면 회전 없이 Scale X만 늘려도 충분함.

        // 레이저 서서히 사라지기
        await ShrinkLazer(lazer);

        boss.OnAnimationTrigger("AttackEnd");
    }
    private async Awaitable ShrinkLazer(GameObject lazerObj)
    {
        float timer = 0f;
        Vector3 initialScale = lazerObj.transform.localScale;
                
        // var hitBox = lazerObj.GetComponent<BasicHitBox>();
        // if(hitBox) hitBox.Activate();

        while (timer < laserDuration)
        {
            if (lazerObj == null) break;

            timer += Time.deltaTime;
            float progress = timer / laserDuration;

            // Y축만 줄어듦 (1 -> 0)
            float currentY = Mathf.Lerp(initialScale.y, 0f, progress);
            lazerObj.transform.localScale = new Vector3(initialScale.x, currentY, initialScale.z);

            await Awaitable.NextFrameAsync(boss.DestroyCancellationToken);
        }

        if (lazerObj != null) Destroy(lazerObj);
    }


    public override void UpdatePattern() { }
    public override void ExitPattern() { }
    public override void OnAnimationEvent(string eventName) { }
}