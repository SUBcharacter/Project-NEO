using UnityEngine;

public class Handgun : Weapon
{
    [SerializeField] Transform muzzle;

    protected override void Awake()
    {
        firing = false;
        player = FindAnyObjectByType<Player>();
        ren = GetComponentsInChildren<SpriteRenderer>();
        mag = GetComponentInChildren<Magazine>();
        foreach(var r in ren)
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
        mag.Fire(dir, muzzle.position, player.SkMn.Enhanced);
    }

    
}
