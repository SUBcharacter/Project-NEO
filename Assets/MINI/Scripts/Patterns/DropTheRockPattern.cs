using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DropTheRockPattern", menuName = "Boss/Patterns/DropTheRock")]
public class DropTheRockPattern : BossPattern
{
    [Header("Prefab Setting")]
    [SerializeField] private GameObject rockPrefab;        // FallingRock스크립트가 붙은 돌 프리팹
    [SerializeField] private LayerMask terrainLayer;

    [Header("Pattern Settings")]
    [SerializeField] private int rockCount = 6;
    [SerializeField] private float initSpeed = 5f;
    [SerializeField] private float spawnHeightOffset = 12f;
    [SerializeField] private float maxDropDelay = 0.3f;        // 랜덤 시간차 (0 ~ 1초 사이)


    private List<GameObject> activeRocks = new List<GameObject>();  // 프리팹 관리용 리스트

    public override async void StartPattern()
    {
        if (boss == null) return;
        activeRocks.Clear();


        if (terrainLayer == 0) terrainLayer = LayerMask.GetMask("Terrain", "Ground");

        // 전조 동작 
        boss.animator.SetTrigger("DropTheRockRaw");
        
        // 쉐킷 쉐쉐킷
        CameraShake.instance.Shake(0.3f, 2f);

        // 대기
        try
        {
            await Awaitable.WaitForSecondsAsync(2f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        // 범위 계산
        (float minX, float maxX) = ObtainRange(boss.transform.position.x);

        // 공격 
        boss.animator.SetTrigger("HitTheGround");

        // 2차 쉐킷 쉐쉐킷
        CameraShake.instance.Shake(0.6f, 0.3f);

        // 돌멩이 투하!
        DropTheRock(minX, maxX);

        // 종료 대기 (돌이 다 떨어지고 정리될 시간만큼 대기)
        try
        {
            await Awaitable.WaitForSecondsAsync(maxDropDelay + 2.0f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        boss.OnAnimationTrigger("AttackEnd");
        ExitPattern();
    }

    // 좌우 벽을 탐지해서 돌이 맵 밖으로 안 나가게 범위 계산
    (float, float) ObtainRange(float bossX)
    {
        float rockRadius = 0.5f;
        float rangeLimit = 50f;

        float safetyMargin = 1f;

        if (rockPrefab != null)
        {
            var col = rockPrefab.GetComponent<Collider2D>();
            if (col != null)
            {
                //Debug.Log(col);
                rockRadius = col.bounds.extents.x + safetyMargin;
            }            
        }
        float checkY = boss.transform.position.y + spawnHeightOffset;
        Vector2 rayOrigin = new(boss.transform.position.x, checkY);

        // 디버그용
        Debug.DrawRay(rayOrigin, Vector2.left * rangeLimit, Color.red, 1.0f);
        Debug.DrawRay(rayOrigin, Vector2.right * rangeLimit, Color.red, 1.0f);

        // 왼쪽 검사
        float x1 = bossX - rangeLimit;
        RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, rangeLimit, terrainLayer);
        if (hitLeft.collider != null) 
            x1 = hitLeft.point.x + rockRadius;

        // 오른쪽 검사
        float x2 = bossX + rangeLimit;
        RaycastHit2D hitRight = Physics2D.Raycast(rayOrigin, Vector2.right, rangeLimit, terrainLayer);
        if (hitRight.collider != null) 
            x2 = hitRight.point.x - rockRadius;

        // 예외 처리
        if (x1 > x2) { x1 = bossX; x2 = bossX; }

        return (x1, x2);
    }

    void DropTheRock(float x1, float x2)
    {
        for (int i = 0; i < rockCount; i++)
        {            
            float randomX = Random.Range(x1, x2);
            float randomY = Random.Range(boss.transform.position.y + 10f, boss.transform.position.y+spawnHeightOffset);
            
            Vector3 spawnPos = new(randomX, randomY, 0);

            // 랜덤 시간차 선정
            float delay = Random.Range(0f, maxDropDelay);

            // 비동기로 생성 처리
            ProcessSingleRock(spawnPos, delay);
        }
    }

    private async void ProcessSingleRock(Vector3 pos, float delay)
    {
        if (boss == null) return;

        try
        {
            // 랜덤 대기
            await Awaitable.WaitForSecondsAsync(delay, boss.DestroyCancellationToken);

            // 돌 생성
            GameObject rock = Instantiate(rockPrefab, pos, Quaternion.identity);
            activeRocks.Add(rock);

            // Rock 초기화 
            var rockScript = rock.GetComponent<FallingRock>();
            if (rockScript != null)
            {
                rockScript.Init();
            }  

            var rb = rock.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.down * initSpeed;
            }
        }
        catch (System.OperationCanceledException) 
        {
            Debug.Log("DropTheRockPattern 취소됨");
            return;
        }
    }

    public override void UpdatePattern() { }

    public override void ExitPattern()
    {
        // 공중에 남은 돌들 정리
        for (int i = activeRocks.Count - 1; i >= 0; i--)
        {
            if (activeRocks[i] != null) Destroy(activeRocks[i]);
        }
        activeRocks.Clear();
    }
    public override void OnAnimationEvent(string eventName) { }
}