using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource moveAudio;
    [SerializeField] AudioSource actAudio;

    [Header("Audio Clips")]
    [SerializeField] AudioClip move;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip attack1;
    [SerializeField] AudioClip attack2;
    [SerializeField] AudioClip attack3;
    [SerializeField] AudioClip dodge;
    [SerializeField] AudioClip hit;
    [SerializeField] AudioClip death;


    public void Move()
    {
        moveAudio.PlayOneShot(move);
    }

    public void Jump()
    {
        moveAudio.PlayOneShot(jump);
    }

    public void Dodge()
    {
        actAudio.PlayOneShot(dodge);
    }

    public void Attack1()
    {
        actAudio.PlayOneShot(attack1);
    }

    public void Attack2()
    {
        actAudio.PlayOneShot(attack2);
    }

    public void Attack3()
    {
        actAudio.PlayOneShot(attack3);
    }

    public void Hit()
    {
        actAudio.PlayOneShot(hit);
    }

    public void Death()
    {
        actAudio.PlayOneShot(death);
    }
}
