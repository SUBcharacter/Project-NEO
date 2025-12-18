using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeControler : MonoBehaviour
{
    [SerializeField] Image fade;
    public static FadeControler instance;

    private void Awake()
    {
        instance = this;
        fade = GetComponentInChildren<Image>();
    }

    public void TryFadeIn(float duration)
    {
        StartCoroutine(Fade(1f, 0f, duration));
    }

    public void TryFadeOut(float duration)
    {
        StartCoroutine(Fade(0f, 1f, duration));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0;

        Color color = fade.color;

        while(timer < duration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            fade.color = new Color(color.r, color.g, color.b, alpha);

            yield return null;
        }

        fade.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
