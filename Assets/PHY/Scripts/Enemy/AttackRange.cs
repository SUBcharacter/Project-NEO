using UnityEngine;

public class AttackRange : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 공격 범위 내에 있는지 판단하는 스크립트
    /// </summary>
    public bool isPlayerInAttackRange { get; private set; }
    public Transform target { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            isPlayerInAttackRange = true;
            target = collision.transform;
        }
    }

}
