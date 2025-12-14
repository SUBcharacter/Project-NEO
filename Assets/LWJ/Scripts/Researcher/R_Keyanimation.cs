using UnityEngine;

public class R_Keyanimation : MonoBehaviour
{
    Researcher researcher;

    void Awake()
    {
        researcher = GetComponentInParent<Researcher>();
    }   

    public void Summontoattack()
    {
        researcher.summontoattack();
    }

    public void Die()
    {
        researcher.R_die();
    }
}
