using UnityEngine;

public class BossStats : MonoBehaviour
{
    [SerializeField] private float maxHP = 500;
    private float currentHP;
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

        Debug.Log("Boss dead");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            Debug.Log("Boss taked Damage");
            Damage(25);
            Debug.Log("Boss HP : " + currentHP);
        }
    }
}
