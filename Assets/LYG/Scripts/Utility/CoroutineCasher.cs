using System.Collections.Generic;
using UnityEngine;

public class CoroutineCasher : MonoBehaviour
{
    static Dictionary<float, WaitForSeconds> casher = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds Wait(float cash)
    {
        if(casher.ContainsKey(cash))
        {
            return casher[cash];
        }
        casher[cash] = new WaitForSeconds(cash);
        return casher[cash];
    }
}
