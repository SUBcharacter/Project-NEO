using UnityEngine;

public class MeleeAndroidEvent : MonoBehaviour
{
    [SerializeField] EnhancableMelee em;

    private void Awake()
    {
        em = GetComponentInParent<EnhancableMelee>();
    }

    public void OnAttacking()
    {
        em.Attacking = true;
    }

    public void EndAttacking()
    {
        em.Attacking = false;
    }

    public void StartAttack()
    {
        em.Attack();
    }

    public void Enhancing()
    {
        em.Enhanced = true;
    }

    public void OnIsDeath()
    {
        em.IsDead = true;
    }
}
