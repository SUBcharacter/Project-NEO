using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] List<GameObject> pools;

    [SerializeField] int size;
    [SerializeField] int index;

    private void Awake()
    {
        index = 0;
        pools = new List<GameObject>();
        for(int i= 0; i<size; i++)
        {
            GameObject instance = Instantiate(bullet,transform);
            pools.Add(instance);
            pools[i].SetActive(false);
        }
    }

    public void Fire(Vector2 dir, Vector3 pos)
    {
        pools[index].GetComponent<Bullet>().Init(dir, pos);
        index = (index + 1) % size;
    }
}
