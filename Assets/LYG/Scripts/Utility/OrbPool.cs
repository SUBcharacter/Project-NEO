using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbPool : MonoBehaviour
{
    [SerializeField] Orb orbPrefab;
    [SerializeField] List<Orb> pool;

    [SerializeField] int index;
    [SerializeField] int size;

    private void Awake()
    {
        for(int i = 0; i< size; i++)
        {
            Orb instance = Instantiate(orbPrefab, transform);
            pool.Add(instance);
            pool[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SpawnOrb(mousePos); 
        }
    }

    public void SpawnOrb(Vector3 pos)
    {
        pool[index].GetComponent<Orb>().Init(pos);
        index = (index + 1) % size;
    }

}
