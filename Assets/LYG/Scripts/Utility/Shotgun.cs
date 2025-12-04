using UnityEngine;

public class Shotgun : Weapon
{
    [SerializeField] Transform muzzle;

    [SerializeField] int pellet;

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
        CameraShake.instance.Shake(5, 0.2f);
        for (int i = 0; i < pellet; i++)
        {
            Vector2 originDir = dir;
            float rand = Random.Range(-20f, 20f);
            originDir = Quaternion.Euler(0, 0, rand) * originDir;
            mag.Fire(originDir, muzzle.position);
        }
    }
}
