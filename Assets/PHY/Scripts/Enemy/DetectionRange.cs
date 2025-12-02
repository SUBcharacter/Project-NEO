using UnityEngine;

/// <summary>
/// 문제 아닌 문제점 발생 : 플레이어가 시야범위 밖에서 쏴도 적이 인식하여 총을 쏴버림
/// 불렛 레이어나 다른 방어코드가 필요할 거 같음
/// </summary>

public class DetectionRange : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 시야 범위 내에 있는지 판단하는 스크립트
    /// </summary>
    public bool isPlayerInRange { get; private set; }
    public Transform target { get; private set; }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            isPlayerInRange = true;
            target = collision.transform;
            Debug.Log("플레이어가 시야 범위에 들어옴");
        }
    }
}
