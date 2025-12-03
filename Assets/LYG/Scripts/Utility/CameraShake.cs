using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    [SerializeField] CinemachineCamera cm;
    [SerializeField] CinemachineBasicMultiChannelPerlin perlin;
    [SerializeField] float shakeTimer;

    private void Awake()
    {
        instance = this;
        cm = GetComponent<CinemachineCamera>();
        perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if(shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            if(shakeTimer <= 0)
            {
                perlin.AmplitudeGain = 0f;
            }
        }
    }

    public void Shake(float intensity, float time)
    {
        perlin.AmplitudeGain = intensity;
        shakeTimer = time;
    }
}
