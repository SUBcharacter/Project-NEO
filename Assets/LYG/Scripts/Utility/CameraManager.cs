using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] CinemachineCamera cm;
    [SerializeField] CinemachinePositionComposer cmp;
    [SerializeField] CinemachineBasicMultiChannelPerlin perlin;
    Coroutine shake;
    Coroutine zoom;

    private void Awake()
    {
        instance = this;
        cm = GetComponent<CinemachineCamera>();
        cmp = GetComponent<CinemachinePositionComposer>();
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

    public void SetDamping(Vector2 value)
    {
        cmp.Damping = value;
    }

    public void DeadZoneControl(Vector2 size)
    {
        size.x = Mathf.Clamp(size.x, 0, 2);
        size.y = Mathf.Clamp(size.y, 0, 2);
        cmp.Composition.DeadZone.Size = size;
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
        float targetSize = Mathf.Clamp(amount, 0.5f, 20f);

        float startSize = cm.Lens.OrthographicSize;
        float timer = 0;
        
        while(timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            cm.Lens.OrthographicSize = Mathf.Lerp(cm.Lens.OrthographicSize, targetSize, t);
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
