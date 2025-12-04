using UnityEngine;

public class Minigun : Weapon
{
    [SerializeField] Transform muzzle;

    protected override void Awake()
    {
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
        CameraShake.instance.Shake(4, 0.1f);
        float rand = Random.Range(-3f, 3f);
        dir = Quaternion.Euler(0, 0, rand) * dir;
        mag.Fire(dir, muzzle.position);
    }
}
