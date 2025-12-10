using UnityEngine;

public class ArmEvent : MonoBehaviour
{
    Researcher researcher;
    void Start()
    {
        researcher = GetComponentInParent<Researcher>();    
    }

    public void R_Shoot()
    {
        researcher.ShootBullet();
    }
}
