using UnityEngine;

public class EnemyBullet : Bullet
{
    /// <summary>
    /// Bullet.sc가 공용 스크립트 인거 같아서 상속받아 사용함 
    /// </summary>

    public void Fire(Vector3 startPos, Vector3 targetPos)
    {
        Vector2 direction = (targetPos - startPos).normalized;
        Init(direction, startPos);
    }
}

// 오버랩 스피어로 추후 수정
// 콜라이더보단 오버랩 스피어로 해라
// 콜라이더나 트리거기반으로 하면 연산작용이 덜 들어가지만 그만큼의 코드들이 더 늘어나고 추후 문제들이 생길 수 있기에 오버랩 스피어로 하는 게 낫다 ㅇㅋㅇㅋ