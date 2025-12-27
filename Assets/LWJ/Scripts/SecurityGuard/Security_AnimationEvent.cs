using UnityEngine;

public class Security_AnimationEvent : MonoBehaviour
{
    [SerializeField] Security_Guard sg;

    private void Awake()
    {
        sg = GetComponentInParent<Security_Guard>();
    }

    public void onattack()
    {
        sg.isattack = true;
    }

    public void endattack()
    {
        sg.isattack = false;
    }   

    public void Startattack()
    {
        sg.Attack();
    }

    public void Die()
    {
        sg.guarddie();
    }
}
