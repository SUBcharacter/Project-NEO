using UnityEngine;

public class EnemyBullet : Bullet
{
    [SerializeField] private BulletStat enemyBulletStat;
    //[SerializeField] private LongDiEnemy shooter;
    private LayerMask layer;

    public void Fire(Vector3 startPos, Vector3 targetPos)
    {
        stats = enemyBulletStat;

        Vector2 direction = (targetPos - startPos).normalized;
        Init(direction, startPos);
    }

    protected override void Triggered(Collider2D collision)
    {
        base.Triggered(collision);

        int layerMask = 1 << collision.gameObject.layer;

        // 벽 충돌 체크 (피디가 쓰는 방식 그대로)
        if ((layerMask & layer) > 0)
        {
            transform.SetParent(parent);
            gameObject.SetActive(false);
            return;
        }

        // 공격 가능한 대상 체크
        //if ((layerMask & stats.attackable) > 0)
        //{
        //    if (shooter != null)
        //        shooter.HitEnemy();
        //
        //    transform.SetParent(parent);
        //    gameObject.SetActive(false);
        //    return;
        //}
    }
}