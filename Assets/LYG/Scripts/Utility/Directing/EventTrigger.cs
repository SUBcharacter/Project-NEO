using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    [SerializeField] Collider2D col;
    [SerializeField] Director director;

    [SerializeField] LayerMask collisionMask;

    [SerializeField] int sequanceNum;
    [SerializeField] bool isTriggered;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        director = FindAnyObjectByType<Director>();
    }

    void Triggered(GameObject collision)
    {
        if (((1 << collision.gameObject.layer) & collisionMask) == 0)
            return;
        director.Play(sequanceNum);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("트리거 이벤트");
        Triggered(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("트리거 이벤트");
        Triggered(collision.gameObject);
    }
}
