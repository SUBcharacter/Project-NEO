using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoubleTap : Weapon
{
    [SerializeField] Transform[] muzzle;
    [SerializeField] Light2D[] muzzleFlash;

    protected override void Awake()
    {
        firing = false;
        player = FindAnyObjectByType<Player>();
        mag = GetComponentInChildren<Magazine>();
        ren = GetComponentsInChildren<SpriteRenderer>();
        muzzleFlash = GetComponentsInChildren<Light2D>();
        foreach (var r in ren)
        {
            r.enabled = false;
        }
        foreach(var m in muzzleFlash)
        {
            m.enabled = false;
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
            mag.Fire(dir, m.position, player.SkMn.Enhanced);
        }
        StartCoroutine(MuzzleFlash());
    }

    IEnumerator MuzzleFlash()
    {
        foreach (var m in muzzleFlash)
        {
            m.enabled = true;
        }

        yield return CoroutineCasher.Wait(0.01f);

        foreach (var m in muzzleFlash)
        {
            m.enabled = false;
        }
    }
}
