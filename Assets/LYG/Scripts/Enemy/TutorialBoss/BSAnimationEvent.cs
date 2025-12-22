using UnityEngine;

public class BSAnimationEvent : MonoBehaviour
{
    [SerializeField] Bisili bs;

    private void Awake()
    {
        bs = GetComponentInParent<Bisili>();
    }

    public void Attacking(int value)
    {
        bs.Attacking = value == 1 ? true : false;
    }


    public void Swing()
    {
        bs.StartAttack();
    }
}
