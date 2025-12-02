using UnityEngine;

public class LongDiEnemyBase : MonoBehaviour
{
    [Header("Enemy 이동관련 변수")]
    [SerializeField] protected float moveSpeed = 1.5f;
    [SerializeField] protected float moveDistance = 2.5f;
    protected bool isMovingRight = true;
    protected Vector3 startPos;

    [Header("Enemy 체력관련 변수")]
    [SerializeField] protected int maxHits = 4;
    protected int currentHits = 0;

    // 내부 컴포넌트
    protected Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;

    protected void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    // 좌우로 이동하는 함수 
    protected void Move()
    {
        float moveDir = isMovingRight ? 1f : -1f;
        rigid.linearVelocity = new Vector2(moveDir * moveSpeed, rigid.linearVelocity.y);

        float dis = transform.position.x - startPos.x;
        // Mathf.Abs : 절댓값 반환 (양수 음수 안가리고 이동한 거리 비교를 위해 사용
        if (Mathf.Abs(dis) >= moveDistance)
        {
            isMovingRight = !isMovingRight;
            startPos = transform.position;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBullet>() != null) return;

        // 태그가 Defaultd인 이유 : 1차 머지아닌 머지를 했을 때 넘겨받은 플레이어가 Default 레이어였음..
        // 추후 머지할 때 수정된 플레이어가 넘어온다면 태그 이름도 바뀔 예정
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            TakeDamage();

        }
    }
    /// <summary>
    /// 적이 데미지를 입는 함수
    /// </summary>
    protected void TakeDamage()
    {
        currentHits++;
        Debug.Log($"맞은 횟수 : {currentHits}");
        if (currentHits >= maxHits)
            Die();
    }

    /// <summary>
    /// 적이 죽었을 때 함수
    /// </summary>
    protected void Die()
    {
        // 죽는 애니메이션은 구현 못한 관계로 비활성화로 대체함
        gameObject.SetActive(false);
        Debug.Log("요원 사망");
    }
}
