using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbPool : MonoBehaviour
{
    [SerializeField] GameObject orbPrefab;
    [SerializeField] List<GameObject> pool;

    [SerializeField] int index;
    [SerializeField] int size;

    private void Awake()
    {
        for(int i = 0; i< size; i++)
        {
            GameObject instance = Instantiate(orbPrefab, transform);
            pool.Add(instance);
            pool[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SpawnOrb(mousePos); 
        }
    }

    public void SpawnOrb(Vector3 pos)
    {
        pool[index].GetComponent<BulletOrb>().Init(pos);
        index = (index + 1) % size;
    }

}
