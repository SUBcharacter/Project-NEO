using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LightWavePattern", menuName = "Boss/Patterns/LightWave")]
public class LightWavePattern : BossPattern
{
    [Header("Prefab")]
    [SerializeField] private GameObject lightWaveObject;

    [Header("Setting")]
    [SerializeField] private Vector2 objectSpawnOffset;         // 보스 기준 오브젝트 생성 위치
    [SerializeField] private int maxObejects = 2;                  // 최대 생성 오브젝트 수


    //[System.NonSerialized] private GameObject spawnCoreObject;
    public override async void StartPattern()
    {
        if (boss == null)
        {
            Debug.LogError("LightWavePattern의 boss참조가 Null임");
            return;
        }
        boss.activeLightWaves.RemoveAll(x => x == null); // 참조 정리

        if (boss.activeLightWaves.Count >= maxObejects)
        {
            boss.OnAnimationTrigger("AttackEnd");
            return;
        }

        Init();

        //boss.animator.SetTrigger("LightWave");

        float direction = Mathf.Sign(boss.transform.localScale.x);
        Vector3 finalSpawnOffset = new(objectSpawnOffset.x * direction, objectSpawnOffset.y, 0f);
        Vector3 SpawnPos = boss.transform.position + finalSpawnOffset;

        try
        {
            await Awaitable.WaitForSecondsAsync(0.5f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { ExitPattern(); return; }

        GameObject newObj = Instantiate(lightWaveObject, SpawnPos, Quaternion.identity);
        boss.activeLightWaves.Add(newObj);

        boss.OnAnimationTrigger("AttackEnd");
    }


    public override void UpdatePattern()
    {
    }

    public override void ExitPattern()
    {

    }
    public override void OnAnimationEvent(string eventName)
    {

    }
    void Init()
    {
        if (boss == null)
        {
            Debug.LogError("LightWavePattern의 boss참조가 Null임");
            return;
        }
        objectSpawnOffset = new Vector2(1.5f * boss.transform.localScale.x, 0f);

    }

}
