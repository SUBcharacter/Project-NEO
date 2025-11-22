using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private int maxHP = 500;
    private int currentHP;
    private bool isDead = false;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void Damage(int damage)
    {
        if(isDead) return;

        currentHP -= damage;

        if(currentHP <= 0)
        {
            Dead();
        }
    }
    public void Dead()
    {
        if(isDead) return;

        isDead = true;
    }
}
