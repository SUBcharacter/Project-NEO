using UnityEngine;

public abstract class TungTungEState
{
    public abstract void Start(TungTungE tte);

    public abstract void Update(TungTungE tte);

    public abstract void Exit(TungTungE tte);
}

public class TTEBattleIdleState : TungTungEState
{
    float timer;

    public override void Start(TungTungE tte)
    {
        Debug.Log("전투 대기");
        timer = 0;
    }

    public override void Update(TungTungE tte)
    {
        timer += Time.deltaTime;
        if(timer >= tte.Stat.waitTime)
        {
            if(tte.DistanceToPlayer() <= tte.Stat.attackDistance)
            {
                tte.ChangeState(tte.State["Attack"]);
            }
            else
            {
                tte.ChangeState(tte.State["Chase"]);
            }
        }


    }

    public override void Exit(TungTungE tte)
    {
        
    }

}

public class TTEAttackState : TungTungEState
{

    public override void Start(TungTungE tte)
    {
        tte.StartAttack();
    }

    public override void Update(TungTungE tte)
    {
        if (!tte.Attacking)
        {
            tte.ChangeState(tte.State["BattleIdle"]);
        }
    }

    public override void Exit(TungTungE tte)
    {

    }
}

public class TTEChasingState : TungTungEState
{
    Vector2 duckingDir;
    float speed;

    public override void Start(TungTungE tte)
    {
        speed = 0;
        duckingDir = tte.FacingRight ? Vector2.right : Vector2.left;
    }

    public override void Update(TungTungE tte)
    {
        //speed = Mathf.Lerp(speed, 0, 5 * Time.deltaTime);
        //tte.Rigid.linearVelocityX = duckingDir.x * speed;
        //if (speed <= 2f)
        //{
        //    tte.Rigid.linearVelocity = Vector2.zero;
        //    tte.ChangeState(tte.State["Attack"]);
        //{
        if (tte.DistanceToPlayer() <= tte.Stat.attackDistance)
        {
            tte.ChangeState(tte.State["Attack"]);
        }
        else
        {
            speed = Mathf.Lerp(speed, tte.Stat.speed, 5 * Time.deltaTime);
            tte.Rigid.linearVelocityX = duckingDir.x * speed;
        }

    }

    public override void Exit(TungTungE tte)
    {
        tte.Rigid.linearVelocityX = 0;
    }

    
}

public class TTESwayState : TungTungEState
{

    public override void Start(TungTungE tte)
    {

    }

    public override void Update(TungTungE tte)
    {

    }

    public override void Exit(TungTungE tte)
    {

    }
}

public class TTEHitState : TungTungEState
{
    float timer;
    public override void Start(TungTungE tte)
    {
        timer = 0;
        tte.StopAttack();
    }

    public override void Update(TungTungE tte)
    {
        timer += Time.deltaTime;
        if(timer >= tte.Stat.hitDuration)
        {
            if(tte.DistanceToPlayer() <= tte.Stat.attackDistance)
            {
                tte.ChangeState(tte.State["Attack"]);
            }
            else
            {
                tte.ChangeState(tte.State["Chase"]);
            }
        }
    }

    public override void Exit(TungTungE tte)
    {

    }
}

public class TTEDeathState : TungTungEState
{

    public override void Start(TungTungE tte)
    {

    }

    public override void Update(TungTungE tte)
    {

    }

    public override void Exit(TungTungE tte)
    {

    }
}

