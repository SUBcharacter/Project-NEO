using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] SpriteRenderer ren;

    private void Awake()
    {
        ren = GetComponentInChildren<SpriteRenderer>();
        ren.enabled = false;
    }

    public void EnableSprite(bool value)
    {
        // 스프라이트 상태 변환
        ren.enabled = value;
    }
}
