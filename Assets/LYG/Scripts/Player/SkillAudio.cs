using UnityEngine;

public class SkillAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource source;

    [Header("Audio Clips")]
    [SerializeField] AudioClip chargeAttack;
    [SerializeField] AudioClip autoTargeting;
    [SerializeField] AudioClip flashAttack;

    public void ChargeAttack()
    {
        source.PlayOneShot(chargeAttack);
    }

    public void AutoTargeting()
    {
        source.PlayOneShot(autoTargeting);
    }

    public void FlashAttack()
    {
        source.PlayOneShot(flashAttack);
    }
}
