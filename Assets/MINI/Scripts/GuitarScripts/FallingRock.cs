using System.Collections;
using UnityEngine;

public class FallingRock : BasicHitBox
{
    [Header("Visual Effects")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private GameObject dustPrefab;

    private GameObject currentWarning;
    private Rigidbody2D rb;
    private bool isImpacted = false;

    public override void Init(bool enhanced = false)
    {
        base.Init(); 
        rb = GetComponent<Rigidbody2D>();
        isImpacted = false;

        ShowWarningIndicator();
    }

    private void ShowWarningIndicator()
    {
        if (warningPrefab == null) return;
        if (stats == null)
        {           
            return;
        }
        int terrainMask = LayerMask.GetMask("Terrain", "Ground"); // 경고 장판은 바닥에서만 생겨야해서 추가한 LayerMask
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50f, terrainMask);

        if (hit.collider != null)
        {
            // Pivot이 Bottom이라면 hit.point 그대로 사용
            currentWarning = Instantiate(warningPrefab, hit.point, Quaternion.identity);
        }
    }

    protected override void Triggered(GameObject collision)
    {
        if (isImpacted) return;

        base.Triggered(collision);

        int layer = collision.gameObject.layer;

        
        if (layer == (int)Layers.terrain || layer == (int)Layers.enviroment)
        {
            OnImpact();
        }
        // 플레이어에 닿아도 돌은 계속 떨어져야 하는가? -> 보통은 관통해서 바닥에 박힘.
        // 따라서 플레이어 충돌 시에는 파괴 로직을 실행하지 않음 (데미지만 줌)
    }

    private void OnImpact()
    {
        isImpacted = true;

        // 물리 간섭 제거 
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.simulated = false; 
        }

        // 경고 표시 즉시 삭제
        if (currentWarning != null) Destroy(currentWarning);

        // 먼지 이펙트 생성
        if (dustPrefab != null)
        {
            Instantiate(dustPrefab, transform.position, Quaternion.identity);
        }

        // 돌멩이 삭제 HitBox 끄려고
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol) myCol.enabled = false;

        StartCoroutine(DestroyRoutine());
    }
    IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (currentWarning != null) Destroy(currentWarning);
    }
}