using System.Collections;
using UnityEngine;

public class Shotgun : Weapon
{
    [SerializeField] Transform muzzle;

    [SerializeField] int pellet;

    protected override void Awake()
    {
        firing = false;
        player = FindAnyObjectByType<Player>();
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
            float rand = Random.Range(-60f, 60f);
            originDir = Quaternion.Euler(0, 0, rand) * originDir;
            mag.Fire(originDir, muzzle.position, player.SkMn.Enhanced);
        }
        StartCoroutine(Recoil());
        player.Rigid.linearVelocity = Vector2.zero;
        player.Rigid.linearVelocity += (-dir * 6);
    }

    IEnumerator Recoil()
    {
        firing = true;
        yield return CoroutineCasher.Wait(0.5f);
        firing = false;
    }
}
