using UnityEngine;

public class EnemyBullet : Bullet
{
    // Bullet.sc가 공용 스크립트 인거 같아서 상속받아 사용
    public void Fire(Vector3 startPos, Vector3 targetPos)
    {
        Vector2 direction = (targetPos - startPos).normalized;
        Init(direction, startPos);
    }
}
