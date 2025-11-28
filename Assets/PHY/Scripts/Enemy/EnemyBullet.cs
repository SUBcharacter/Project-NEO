using UnityEngine;

public class EnemyBullet : Bullet
{

    public void Fire(Vector3 startPos, Vector3 targetPos)
    {
        Vector2 direction = (targetPos - startPos).normalized;
        Init(direction, startPos);
    }
}
