using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] SkillStat phantomBlade;
    [SerializeField] Player player;
    public Magazine knifePool;

    public float phantomBladeTimer;

    public bool phantomBladeUsable;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        knifePool = GetComponent<Magazine>();
        phantomBladeUsable = true;
    }

    private void Update()
    {
        PhantomBladeCoolTime();
    }

    void PhantomBladeCoolTime()
    {
        if (phantomBladeUsable)
            return;
        phantomBladeTimer += Time.deltaTime;

        if(phantomBladeTimer >= phantomBlade.coolTime)
        {
            phantomBladeUsable = true;
            phantomBladeTimer = 0;
        }
    }

    public void InitiatingPhantomBlade(Transform[] spawnPoint, Vector2 dir)
    {
        if (!phantomBladeUsable)
        {
            Debug.Log("쿨다운");
            return;
        }
        phantomBladeUsable = false;
        player.casting = true;
        StartCoroutine(PhantomBlade(spawnPoint, dir));
    }

    public IEnumerator PhantomBlade(Transform[] spawnPoint, Vector2 dir)
    {
        if (player.stamina >= phantomBlade.staminaCost)
        {
            player.stamina -= phantomBlade.staminaCost;
            int index;
            List<int> usedIndex = new();
            GameObject[] knives = new GameObject[phantomBlade.attackCount];
            for (int i = 0; i < phantomBlade.attackCount; i++)
            {
                while (true)
                {
                    index = Random.Range(0, spawnPoint.Length);

                    if (!usedIndex.Contains(index))
                        break;
                }

                usedIndex.Add(index);
                knives[i] = knifePool.Fire(dir, spawnPoint[index].position);

                yield return CoroutineCasher.Wait(0.2f);
            }

            yield return CoroutineCasher.Wait(0.1f);

            foreach (var k in knives)
            {
                k.GetComponent<Knife>().Shoot(dir);
            }
            player.casting = false; 
        }
        else
        {
            Debug.Log("스태미나 부족");
            player.casting = false;
        }
    }

    //public IEnumerator ReadyToCharging()
    //{
    //    while(true)
    //    {
    //
    //    }
    //
    //}
}
