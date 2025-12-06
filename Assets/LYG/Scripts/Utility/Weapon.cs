using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected Player player;
    [SerializeField] protected SpriteRenderer[] ren;
    [SerializeField] protected Magazine mag;
    [SerializeField] protected ShotMode mode;

    [SerializeField] protected bool firing;

    public ShotMode Mode => mode;
    public bool Firing => firing;

    protected abstract void Awake();

    public abstract void EnableSprite(bool value);

    public abstract void Launch(Vector2 dir);
}
