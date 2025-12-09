using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Handgun : Weapon
{
    [SerializeField] Transform muzzle;
    [SerializeField] Light2D muzzleFlash;
    protected override void Awake()
    {
        firing = false;
        player = FindAnyObjectByType<Player>();
        mag = GetComponentInChildren<Magazine>();
        ren = GetComponentsInChildren<SpriteRenderer>();
        muzzleFlash = GetComponentInChildren<Light2D>();
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
        mag.Fire(dir, muzzle.position, player.SkMn.Enhanced);
        StartCoroutine(MuzzleFlash());
    }

    IEnumerator MuzzleFlash()
    {
        muzzleFlash.enabled = true;

        yield return CoroutineCasher.Wait(0.01f);

        muzzleFlash.enabled = false;
    }
}
