using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] CinemachineCamera cm;
    [SerializeField] CinemachineBasicMultiChannelPerlin perlin;
    Coroutine shake;
    Coroutine zoom;

    private void Awake()
    {
        instance = this;
        cm = GetComponent<CinemachineCamera>();
        perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        
    }

    public void Shake(float intensity, float time)
    {
        if (shake != null)
            StopCoroutine(shake);

        shake = StartCoroutine(CameraShake(intensity, time));
    }

    public void ZoomControl(float amount, float duration)
    {
        if (zoom != null)
            StopCoroutine(zoom);

        zoom = StartCoroutine(Zoom(amount, duration));
    }

    public void SetTrackingTarget(Transform target)
    {
        cm.Target.TrackingTarget = target;
    }

    IEnumerator Zoom(float amount, float duration)
    {
        float targetSize = Mathf.Clamp(cm.Lens.OrthographicSize + amount,0.5f, 10f);

        float timer = 0;
        
        while(timer < duration)
        {
            timer += Time.deltaTime;

            cm.Lens.OrthographicSize = Mathf.Lerp(cm.Lens.OrthographicSize, targetSize, timer / duration);
            yield return null;
        }

        cm.Lens.OrthographicSize = targetSize;
        zoom = null;
    }

    IEnumerator CameraShake(float intensity, float time)
    {
        float timer = 0;

        while(timer < time)
        {
            timer += Time.deltaTime;

            perlin.AmplitudeGain = Mathf.Lerp(intensity, 0f, timer / time);

            yield return null;
        }

        perlin.AmplitudeGain = 0f;
        shake = null;
    }
}
