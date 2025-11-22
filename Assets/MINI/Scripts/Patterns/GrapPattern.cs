using UnityEngine;

public class GrapPattern : BossPattern
{

    public GrapPattern(BossAI boss) : base(boss)
    {
    }

    public override async void Start()
    {
        // <--그랩 전 애니메이션 실행시켜준 다음에

        // 이거 실행
        await Awaitable.WaitForSecondsAsync(0.5f);
        DashToPlayer();
    }
    void DashToPlayer()
    {
        Vector3 direction = -(boss.player.localPosition - boss.transform.localPosition);
        float dashSpeed = 10f;
        boss.GetComponent<Rigidbody2D>().linearVelocity = direction * dashSpeed;

        Collider2D hit = Physics2D.OverlapCircle(boss.transform.position, 1f, LayerMask.GetMask("Player"));
        while (hit == null)
        {
            //엄....
        }

        boss.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        boss.player.GetComponent<Rigidbody2D>().linearVelocity = direction * dashSpeed;

    }

    public override void OnAnimationEvent(string eventName)
    {

    }
    public override void Update()
    {

    }
    public override void Exit()
    {

    }
}
