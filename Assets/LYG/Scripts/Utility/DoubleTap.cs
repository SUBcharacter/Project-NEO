using UnityEngine;

public class DoubleTap : Weapon
{
    [SerializeField] Transform[] muzzle;

    protected override void Awake()
    {
        firing = false;
        ren = GetComponentsInChildren<SpriteRenderer>();
        mag = GetComponentInChildren<Magazine>();
        foreach (var r in ren)
        {
            r.enabled = false;
        }
    }

    public override void EnableSprite(bool value)
    {
        foreach (var r in ren)
        {
            r.enabled = value;
        }
    }

    public override void Launch(Vector2 dir)
    {
        foreach(var m in muzzle)
        {
            mag.Fire(dir, m.position);
        }
    }
}
