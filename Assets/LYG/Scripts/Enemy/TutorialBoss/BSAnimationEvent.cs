using UnityEngine;

public class BSAnimationEvent : MonoBehaviour
{
    [SerializeField] Bisili bs;

    private void Awake()
    {
        bs = GetComponentInParent<Bisili>();
    }

    public void StartAttack()
    {
        bs.Attacking = true;
    }

    public void EndAttack()
    {
        bs.Attacking = false;
    }

    public void Swing()
    {
        bs.StartAttack();
    }
}
