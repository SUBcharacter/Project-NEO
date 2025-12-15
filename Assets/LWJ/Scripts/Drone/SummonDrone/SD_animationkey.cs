using UnityEngine;

public class SD_animationkey : MonoBehaviour
{
    SummonDrone summonDrone;
    void Start()
    {
        summonDrone = GetComponentInParent<SummonDrone>();
    }

    public void DroneDie()
    {
        summonDrone.DroneDie();
    }
}
