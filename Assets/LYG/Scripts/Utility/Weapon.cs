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
        ren.enabled = value;
    }
}
