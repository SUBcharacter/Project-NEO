using UnityEngine;
using UnityEngine.XR;

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
        player.aiming = false;
        player.arm.gameObject.SetActive(false);
    }

    public override void Update(Player player)
    {
        
    }

    public override void Exit(Player player)
    {
        
    }
}

public class MeleeAttackState : PlayerState
{
    float timer;
    float relaxTime = 3f;
    public override void Start(Player player)
    {
        timer = 0;
    }

    public override void Update(Player player)
    {
        timer += Time.deltaTime;
        if(timer >= relaxTime)
        {
            player.ChangeState(player.states[0]);
        }
    }

    public override void Exit(Player player)
    {

    }
}

public class RangeAttackState : PlayerState
{
    float timer;
    float RelaxTimer = 3f;

    public override void Start(Player player)
    {
        timer = 0;
        player.aiming = true;
        player.arm.gameObject.SetActive(true);
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

public class ParryingState : PlayerState
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

public class DodgeState : PlayerState
{
    float currentVel;
    float startVel;
    float Yvelocity;
    float gravityScale;

    public override void Start(Player player)
    {
        gravityScale = player.rigid.gravityScale;
        Yvelocity = player.rigid.linearVelocityY;
        player.dodging = true;
        startVel = player.rigid.linearVelocityX;
        player.rigid.gravityScale = 0f;
        player.rigid.linearVelocityY = 0f;
    }

    public override void Update(Player player)
    {
        player.rigid.linearVelocityY = 0f;
        currentVel = player.rigid.linearVelocityX;
        currentVel = Mathf.Lerp(currentVel, 0, 5*Time.deltaTime);
        player.rigid.linearVelocityX = currentVel;
        if(Mathf.Abs(currentVel) <= 1.5f)
        {
            player.rigid.gravityScale = gravityScale;
            player.ChangeState(player.states[0]);
        }
    }

    public override void Exit(Player player)
    {
        player.dodging = false;
        player.rigid.gravityScale = gravityScale;
    }
}
