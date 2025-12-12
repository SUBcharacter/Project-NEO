using System.Collections;
using UnityEngine;

public class SmashFx : BasicHitBox
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
}
