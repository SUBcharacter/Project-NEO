using UnityEngine;

public class D_animationkey : MonoBehaviour
{
    Drone drone;
    void Start()
    {
        drone = GetComponentInParent<Drone>();
    }

    public void Shot()
    {
        drone.Shoot();
    }

    public void AttackEnd()
    {
        drone.isattack = false;
    }

    public void IsEnhance()
    {
        drone.Enhanced = true;
    }

    public void Isdeadend()
    {
        drone.waitgameobjectfalse();
    }



}
