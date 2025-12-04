using UnityEngine;

public class PlayerMove : MonoBehaviour ,IDamageable
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float currentHp;
    [SerializeField] private float maxHp = 100f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {        
        currentHp = maxHp;
    }
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"Player Hit! HP: {currentHp}");
        // 피격 애니메이션이나 무적 시간 로직 추가
    }

    private void Update()
    {
        float hz = Input.GetAxis("Horizontal");
        // float vt = Input.GetAxis("Vertical"); 안쓰지않나?
        Vector3 move = new Vector3(hz, 0, 0) * speed * Time.deltaTime;
        transform.position += move;
    }


}
