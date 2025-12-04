using System.Collections;
using UnityEngine;

public class SmashFx : BaseProjectile
{
    
    private void Start()
    {        
        StartCoroutine(DestroyAfterAnimation());
    }
    IEnumerator DestroyAfterAnimation()
    {
        yield return CoroutineCasher.Wait(0.2f);
        Destroy(gameObject);
    }
    protected override void OnHitPlayer(Collider2D collision)
    {
        Debug.Log(collision.ToString());
        base.OnHitPlayer(collision);
    }
}
