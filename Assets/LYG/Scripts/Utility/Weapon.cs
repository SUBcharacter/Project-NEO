using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer[] ren;
    [SerializeField] protected Magazine mag;
    [SerializeField] protected ShotMode mode;

    public ShotMode Mode => mode;

    protected abstract void Awake();

    public abstract void EnableSprite(bool value);

    public abstract void Launch(Vector2 dir);
}
