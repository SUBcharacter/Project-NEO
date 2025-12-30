using System.Collections;
using UnityEngine;

public class Switch : MonoBehaviour, IDamageable
{
    [SerializeField] Collider2D col;
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Sprite[] onOff;
    [SerializeField] Material hitFlash;
    MaterialPropertyBlock mpb;
    Coroutine hit;

    public bool isOn { get; private set; }

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        ren = GetComponentInChildren<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        ren.GetPropertyBlock(mpb);
        mpb.SetFloat("_FlashAmount", 0.5f);
        ren.SetPropertyBlock(mpb);
        isOn = false;
        ren.sprite = onOff[0];
        col.enabled = true;
    }

    public void TakeDamage(float damage)
    {
        isOn = true;
        col.enabled = false;
        ren.sprite = onOff[1];
        Hit();
    }

    void Hit()
    {
        if(hit != null)
        {
            StopCoroutine(hit);
        }
        hit = StartCoroutine(HitFlash());
    }

    IEnumerator HitFlash()
    {
        Material origin = ren.material;

        ren.material = hitFlash;

        mpb.SetFloat("_FlashAmount", 0.5f);
        ren.SetPropertyBlock(mpb);

        float t = 0;
        float duration = 0.15f;

        while (t <= duration)
        {
            t += Time.deltaTime;
            float flash = Mathf.SmoothStep(0.5f, 0f, t / duration);
            mpb.SetFloat("_FlashAmount", flash);
            ren.SetPropertyBlock(mpb);
            yield return null;
        }

        ren.material = origin;
    }

}
