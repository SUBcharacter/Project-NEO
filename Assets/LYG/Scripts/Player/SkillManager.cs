using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public Magazine knifePool;

    private void Awake()
    {
        knifePool = GetComponent<Magazine>();
    }
}
