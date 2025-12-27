using UnityEngine;

public class TTEAnimationEvent : MonoBehaviour
{
    [SerializeField] TungTungE tte;

    private void Awake()
    {
        tte = GetComponentInParent<TungTungE>();
    }

    public void OnPunch(int index)
    {
        tte.Punches[index].Init();
    }

    public void OffPunch(int index)
    {
        tte.Punches[index].gameObject.SetActive(false);
    }

    public void Attacking(int value)
    {
        tte.Attacking = value == 1 ? true : false;
    }

}
