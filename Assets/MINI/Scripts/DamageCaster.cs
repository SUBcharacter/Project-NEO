using UnityEngine;
using System.Collections.Generic;

public class DamageCaster : BasicHitBox
{
    [SerializeField] private bool oneHitEnable = true;
    private readonly HashSet<GameObject> hitObjects = new();

    private void OnEnable()
    {       
        if (oneHitEnable)
        {
            hitObjects.Clear();
            triggered = false;
        }
    }

    protected override void Triggered(GameObject collision)
    {        
        if (oneHitEnable)
        {
            if (hitObjects.Contains(collision.gameObject)) return;
            hitObjects.Add(collision.gameObject);
        }
        base.Triggered(collision);
    }
}
