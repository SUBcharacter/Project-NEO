using UnityEngine;

public class ElecShockWave : MonoBehaviour
{
    public float lifeTime = 3.5f;


    private void Start()
    {
        var hitbox = GetComponent<ElecHitBox>();
        if (hitbox != null)
        {
            hitbox.Init();

        }

        Destroy(gameObject, lifeTime);
    }


}
