using UnityEngine;

public class DetectionRange : MonoBehaviour
{
    public bool isPlayerInRange { get; private set; }
    public Transform target { get; private set; }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isPlayerInRange = true;
            target = collision.transform;
            Debug.Log("플레이어가 시야 범위에 들어옴");
        }
    }
}
