using UnityEngine;

public class LightWaveObject : MonoBehaviour , IDamageable
{
    [SerializeField] private GameObject lightWaveRing;    

    [SerializeField] public float maxHp;
    [SerializeField] public float currentHp;
    [SerializeField] private float spawnInterval = 5f;
    private float spawnCooltime;

    private void Awake()
    {
        maxHp = 100f;
        currentHp = maxHp;
        spawnCooltime = 0f;
    }
    private void Update()
    {
        if (currentHp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        
        spawnCooltime += Time.deltaTime;
        if (spawnCooltime >= spawnInterval)
        {
            SpawnRing();
            spawnCooltime = 0f;
        }
    }


    void SpawnRing()
    {
        if (lightWaveRing == null)
            return;
        GameObject ring = GameObject.Instantiate(lightWaveRing, transform.position, Quaternion.identity);
        ring.transform.localScale = new(0.1f, 0.1f, 0.1f);
    }      
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
    }
}
