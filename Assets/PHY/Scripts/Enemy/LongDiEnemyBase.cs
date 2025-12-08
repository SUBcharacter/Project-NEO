using UnityEngine;

/// <summary>
/// 에너미 추상 클래스 만들어서 처리하도록 수정하기
/// 히트 박스가 활성화 되는건 총이랑 
/// </summary>
public class LongDiEnemyBase : MonoBehaviour
{




    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    // 좌우로 이동하는 함수 
    protected void Move()
    {
        
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
