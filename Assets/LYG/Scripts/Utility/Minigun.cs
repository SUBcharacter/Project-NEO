using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Minigun : Weapon
{
    [SerializeField] Transform muzzle;

    [SerializeField] Light2D muzzleFlash;

    protected override void Awake()
    {
        firing = false;
        player = FindAnyObjectByType<Player>();
        mag = GetComponentInChildren<Magazine>();
        muzzleFlash = GetComponentInChildren<Light2D>();
        ren = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in ren)
        {
            r.enabled = false;
        }
        muzzleFlash.enabled = false;
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
        CameraManager.instance.Shake(4, 0.1f);
        float rand = Random.Range(-3f, 3f);
        dir = Quaternion.Euler(0, 0, rand) * dir;
        mag.Fire(dir, muzzle.position, player.SkMn.Enhanced);
        StartCoroutine(MuzzleFlash());
    }

    IEnumerator MuzzleFlash()
    {
        muzzleFlash.enabled = true;

        yield return CoroutineCasher.Wait(0.001f);

        muzzleFlash.enabled = false;
    }
}
