using UnityEngine;

public class TTEAnimationEvent : MonoBehaviour
{
    [SerializeField] TungTungE tte;

    private void Awake()
    {
        tte = GetComponentInParent<TungTungE>();
    }

    public void OnLeft()
    {
        tte.Punches[0].Init();
    }

    public void OffLeft()
    {
        tte.Punches[0].gameObject.SetActive(false);
    }

    public void OnRight()
    {
        tte.Punches[1].Init();
    }

    public void OffRight()
    {
        tte.Punches[1].gameObject.SetActive(false);
    }

    public void StartAttack()
    {
        tte.Attacking = true;
    }

    public void EndAttack()
    {
        tte.Attacking = false;
    }
}
