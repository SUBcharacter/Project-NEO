using UnityEngine;

public abstract class PlayerState
{
    public abstract void Start(Player player);
    public abstract void Update(Player player);
    public abstract void Exit(Player player);
}

public class IdleState : PlayerState
{
    public override void Start(Player player)
    {
        
    }

    public override void Update(Player player)
    {
        
    }

    public override void Exit(Player player)
    {
        
    }
}

public class AttackState : PlayerState
{
    float timer;
    float RelaxTimer = 3f;

    public override void Start(Player player)
    {
        timer = 0;   
    }

    public override void Update(Player player)
    {
        player.RotateArm();
        timer += Time.deltaTime;
        if (timer >= RelaxTimer)
        {
            player.ChangeState(player.states[0]);
        }
    }

    public override void Exit(Player player)
    {
        
    }
}

public class SubAttackState : PlayerState
{
    public override void Start(Player player)
    {
        player.aiming = true;
    }

    public override void Update(Player player)
    {
        
    }

    public override void Exit(Player player)
    {
        player.aiming = false;
    }
}
