using UnityEngine;

public abstract class BisiliState
{
    public abstract void Start(Bisili bs);
    public abstract void Update(Bisili bs);
    public abstract void Exit(Bisili bs);
}

public class BSBattleIdleState : BisiliState
{
    float timer;
    public override void Start(Bisili bs)
    {
        timer = 0;
    }

    public override void Update(Bisili bs)
    {
        bs.SpriteControl();
        timer += Time.deltaTime;
        if(timer >= bs.Stat.waitTime)
        {
            if(bs.DistanceToPlayer() <= bs.Stat.attackDistance)
            {
                bs.ChangeState(bs.State["Attack"]);
            }
            else
            {
                bs.ChangeState(bs.State["Chase"]);
            }
        }
    }

    public override void Exit(Bisili bs)
    {
        
    }
}

public class BSChasingState : BisiliState
{
    Vector2 dir;
    float speed;
    public override void Start(Bisili bs)
    {
        speed = 0;
        bs.SpriteControl();
        
    }

    public override void Update(Bisili bs)
    {
        bs.SpriteControl();
        dir = bs.FacingRight ? Vector2.right : Vector2.left;
        if (bs.DistanceToPlayer() <= bs.Stat.attackDistance)
        {
            bs.ChangeState(bs.State["Attack"]);
        }
        else
        {
            speed = Mathf.Lerp(speed, bs.Stat.speed, 5 * Time.deltaTime);
            bs.Rigid.linearVelocityX = dir.x * speed;
        }
        
    }

    public override void Exit(Bisili bs)
    {
        bs.Rigid.linearVelocity = Vector2.zero;
    }
}

public class BSSwayState : BisiliState
{
    Vector2 dir;
    float speed;
    public override void Start(Bisili bs)
    {
        bs.SpriteControl();
        dir = bs.FacingRight ? Vector2.left : Vector2.right;
        speed = bs.Stat.swaySpeed;
    }

    public override void Update(Bisili bs)
    {
        speed = Mathf.Lerp(speed, 0, 5 * Time.deltaTime);
        bs.Rigid.linearVelocityX = dir.x * speed;
        if (speed <= bs.Stat.returnSpeed)
        {
            bs.ChangeState(bs.State["BattleIdle"]);
        }
    }

    public override void Exit(Bisili bs)
    {
        bs.Rigid.linearVelocity = Vector2.zero;
    }
}

public class BSAttackState : BisiliState
{
    float timer;
    public override void Start(Bisili bs)
    {
        timer = 0;
        bs.StartAttack();
    }

    public override void Update(Bisili bs)
    {
        if(!bs.Attacking)
        {
            timer += Time.deltaTime;
            if(timer >= bs.Stat.attackDuration)
            {
                bs.ChangeState(bs.State["Sway"]);
            }
        }
    }

    public override void Exit(Bisili bs)
    {

    }
}

public class BSHitState : BisiliState
{
    float timer;
    public override void Start(Bisili bs)
    {
        timer = 0;
        bs.StopAttack();
    }

    public override void Update(Bisili bs)
    {
        timer += Time.deltaTime;
        if(timer >= bs.Stat.hitDuration)
        {
            if(bs.DistanceToPlayer() <= bs.Stat.attackDistance)
            {
                bs.ChangeState(bs.State["Attack"]);
            }
            else
            {
                bs.ChangeState(bs.State["Chase"]);
            }
        }
    }

    public override void Exit(Bisili bs)
    {

    }
}

public class BSDeathState : BisiliState
{

    public override void Start(Bisili bs)
    {
        bs.gameObject.layer = LayerMask.NameToLayer("Invincible");
        bs.Rigid.linearVelocity = Vector2.zero;
    }

    public override void Update(Bisili bs)
    {

    }

    public override void Exit(Bisili bs)
    {

    }
}