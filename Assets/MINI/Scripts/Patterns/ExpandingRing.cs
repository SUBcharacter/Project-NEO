using UnityEngine;

public class ExpandingRing : BasicHitBox
{
    [SerializeField] private float ringExpandSpeed = 4f;
    [SerializeField] private float maxScale = 10f;

    protected override void Awake()
    {
        base.Awake();
        // stats는 인스펙터에서 HitBoxStat(SO)를 넣어주세요.
    }

    private void Update()
    {
        gameObject.transform.localScale += Vector3.one * ringExpandSpeed * Time.deltaTime;
        if (transform.localScale.x >= maxScale)
        {
            Destroy(gameObject);
        }
    }
    // Triggered 등은 부모(BasicHitBox)가 알아서 플레이어 데미지 처리함
    // 만약 벽에 닿았을 때 사라져야 한다면 오버라이드 필요
    protected override void Triggered(GameObject collision)
    {
        base.Triggered(collision);

        // 벽에 닿으면 삭제되는 로직이 필요하다면:
        //if (triggered && collision.gameObject.layer == (int)Layers.terrain)
        //{
        //    Destroy(gameObject); // 혹은 비활성화
        //}
    }
}
