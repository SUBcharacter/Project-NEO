using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
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
        ren = GetComponentInParent<SpriteRenderer>();
        for(int i = 0; i< size; i++)
        {
            GameObject instance = Instantiate(ghostPrefab, transform);

            pool.Add(instance);
            pool[i].SetActive(false);
        }
    }

    private void Start()
    {
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
        pool[index].GetComponent<Ghost>().SpawnGhost(transform, ren.sprite, ren.flipX);
        index = (index + 1) % pool.Count;
    }
}
