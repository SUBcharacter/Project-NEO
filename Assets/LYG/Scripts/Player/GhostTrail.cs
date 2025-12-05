using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    // 활성화 시 지속적으로 잔상 생성

    [SerializeField] Player player;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] GameObject ghostPrefab;
    [SerializeField] List<GameObject> pool;

    [SerializeField] float timer;
    [SerializeField] float duration;

    [SerializeField] int index;
    [SerializeField] int size;

    private void Awake()
    {
        index = 0;
        player = GetComponentInParent<Player>();
        
        for(int i = 0; i< size; i++)
        {
            GameObject instance = Instantiate(ghostPrefab, transform);

            pool.Add(instance);
            pool[i].SetActive(false);
        }
    }

    private void Start()
    {
        ren = player.Ren;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= duration)
        {
            timer = 0;
            SpawnGhost();
        }
    }

    public void SpawnGhost()
    {
        pool[index].GetComponent<Ghost>().SpawnGhost(player.transform, ren.sprite, ren.flipX);
        index = (index + 1) % pool.Count;
    }
}
