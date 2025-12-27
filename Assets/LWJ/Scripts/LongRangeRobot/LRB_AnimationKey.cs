using UnityEngine;

public class LRB_AnimationKey : MonoBehaviour
{
    LongRobot longRobot;

    private void Awake()
    {
       longRobot = GetComponentInParent<LongRobot>();
    }

    public void Shot()
    {
        longRobot.Shoot();
    }

    public void AttackEnd()
    {
        longRobot.isattack = false;
    }

    public void IsEnhance()
    {
        longRobot.Enhanced = true;
    }

    public void Isdeadend()
    {
        longRobot.waitgameobjectfalse();
    }
}
