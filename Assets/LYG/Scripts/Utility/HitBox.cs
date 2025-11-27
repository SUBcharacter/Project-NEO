using UnityEngine;

public class HitBox : MonoBehaviour
{
    public HitBoxStat stats;

    public bool triggered;
    private void Awake()
    {
        triggered = false;
    }

    public void Init()
    {
        triggered = false;
        gameObject.SetActive(true);
    }

    void Triggered(Collision2D collision)
    {
        // 각 레이어에 맞게 작용하게끔 작성할 것
        if (((1 << collision.gameObject.layer) & stats.attackable) == 0)
            return;
        
        switch (collision.gameObject.layer)
        {
            case (int)Layers.terrain:
                triggered = true;
                Debug.Log("충돌");
                break;
            case (int)Layers.enviroment:
                triggered = true;
                Debug.Log("충돌");
                break;
            case (int)Layers.enemy:
                triggered = true;
                break;
            case (int)Layers.player:
                triggered = true;
                break;
            case (int)Layers.border:
                triggered = true;
                break;
            case (int)Layers.invincible:
                triggered = true;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Triggered(collision);
    }
}
