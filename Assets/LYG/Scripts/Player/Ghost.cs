using UnityEditor.Build;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] SpriteRenderer ren;
    Color baseColor;

    [SerializeField] float timer;
    [SerializeField] float lifeTime;

    private void Awake()
    {
        ren = GetComponentInChildren<SpriteRenderer>();
        baseColor = ren.color;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        float t = Mathf.Clamp01(timer / lifeTime);
        float hue = 1f - t;
        Color c = Color.HSVToRGB(hue, 1f, 0.5f);
        c.a = t;
        ren.color = c;

        if(timer <= 0)
        {
            timer = lifeTime;
            transform.SetParent(parent);
            gameObject.SetActive(false);
        }
    }

    public void SpawnGhost(Transform p, Sprite sr, bool flipX)
    {
        parent = p;
        transform.SetParent(null);
        transform.position = p.position;
        transform.rotation = p.rotation;
        timer = lifeTime;

        ren.sprite = sr;
        ren.flipX = flipX;

        Color c = ren.color;
        c.a = 1f;
        ren.color = c;

        gameObject.SetActive(true);
    }
}
