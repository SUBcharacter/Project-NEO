using System.Threading;
using UnityEngine;

public abstract class EnhancableMeleeState
{
    public abstract void Start(EnhancableMelee melee); 

    public abstract void Update(EnhancableMelee melee); 

    public abstract void Exit(EnhancableMelee melee); 
}

public class EMIdleState : EnhancableMeleeState
{
    float timer;
    float waitTimer = 1;
    public override void Start(EnhancableMelee melee)
    {
        timer = 0;
        melee.Rigid.linearVelocity = Vector2.zero;
        melee.AniCon.Play("EnhancableMelee_Idle");
    }

    public override void Update(EnhancableMelee melee)
    {
        melee.SpriteControl(melee.FacingRight);
        timer += Time.deltaTime;
        
        if(timer >= waitTimer)
        {
            if (melee.Target != null)
            {
                if (melee.DistanceToPlayer() <= melee.Stat.moveDistance)
                {
                    melee.ChangeState(melee.State["Attack"]);
                }
                else
                {
                    melee.ChangeState(melee.State["Chase"]);
                } 
            }
            else
            {
                melee.ChangeState(melee.State["Patrol"]);
            }
        }
    }

    public override void Exit(EnhancableMelee melee)
    {
        
    }
}

public class EMPatrolState : EnhancableMeleeState
{
    float timer;
    float moveTimer;
    public override void Start(EnhancableMelee melee)
    {
        timer = 0;
        moveTimer = Random.Range(0f, 2f);
        bool dirDecision = Random.Range(0, 2) == 1;
        melee.SpriteControl(dirDecision);
        float dir = melee.FacingRight ? 1 : -1;
        melee.Rigid.linearVelocityX = dir * melee.Stat.moveSpeed;
        melee.AniCon.Play("EnhancableMelee_Walk");
    }

    public override void Update(EnhancableMelee melee)
    {
        if(melee.CheckTerrain())
        {
            melee.SpriteControl(!melee.FacingRight);
            melee.Rigid.linearVelocityX *= -1;
        }

        timer += Time.deltaTime;

        if(timer >= moveTimer)
        {
            if(melee.Target != null)
            {
                if(melee.DistanceToPlayer() <= melee.Stat.moveDistance)
                {
                    melee.ChangeState(melee.State["Attack"]);
                }
                else
                {
                    melee.ChangeState(melee.State["Chase"]);
                }
            }
            else
            {
                melee.ChangeState(melee.State["Idle"]);
            }
        }
    }

    public override void Exit(EnhancableMelee melee)
    {

    }
}

public class EMChasingState : EnhancableMeleeState
{
    Vector2 moveDir;

    public override void Start(EnhancableMelee melee)
    {
        melee.AniCon.Play("EnhancableMelee_Walk");
        melee.SpriteControl();
    }

    public override void Update(EnhancableMelee melee)
    {
        melee.SpriteControl();
        moveDir = melee.FacingRight ? Vector2.right : Vector2.left;
        if (melee.Target != null)
        {
            if (melee.DistanceToPlayer() <= melee.Stat.moveDistance)
            {
                melee.ChangeState(melee.State["Attack"]);
            }
            else
            {
                if(melee.CheckTerrain())
                {
                    melee.ChangeState(melee.State["Idle"]);
                }
                else
                {
                    melee.Rigid.linearVelocityX = moveDir.x * melee.Stat.moveSpeed;
                }
            } 
        }
        else
        {
            melee.ChangeState(melee.State["Idle"]);
        }
        
    }

    public override void Exit(EnhancableMelee melee)
    {
        
    }
}

public class EMAttackState : EnhancableMeleeState
{
    float timer;

    public override void Start(EnhancableMelee melee)
    {
        Debug.Log("АјАн");
        timer = 0;
        melee.Rigid.linearVelocity = Vector2.zero;
        melee.SpriteControl();
        melee.Attack();
    }

    public override void Update(EnhancableMelee melee)
    {
        if (!melee.Attacking)
        {
            timer += Time.deltaTime;

            if (timer >= melee.Stat.fireCooldown)
            {
                if (melee.Target != null)
                {
                    if (melee.DistanceToPlayer() <= melee.Stat.moveDistance)
                    {
                        melee.ChangeState(melee.State["Attack"]);
                    }
                    else
                    {
                        melee.ChangeState(melee.State["Chase"]);
                    }
                }
                else
                {
                    melee.ChangeState(melee.State["Idle"]);
                }
            } 
        }
    }

    public override void Exit(EnhancableMelee melee)
    {
        melee.StopAttack();
    }
}

public class EMEnhanceState : EnhancableMeleeState
{

    public override void Start(EnhancableMelee melee)
    {
        melee.StopAttack();
    }

    public override void Update(EnhancableMelee melee)
    {
        if(melee.Enhanced)
        {
            if(melee.Target != null)
            {
                if(melee.DistanceToPlayer() <= melee.Stat.moveDistance)
                {
                    melee.ChangeState(melee.State["Attack"]);
                }
                else
                {
                    melee.ChangeState(melee.State["Chase"]);
                }
            }
            else
            {
                melee.ChangeState(melee.State["Idle"]);
            }
        }
    }

    public override void Exit(EnhancableMelee melee)
    {

    }
}

public class EMHitState : EnhancableMeleeState
{
    float timer;
    float duration = 0.1f;
    public override void Start(EnhancableMelee melee)
    {
        timer = 0;
        melee.StopAttack();
        melee.Rigid.linearVelocity = Vector2.zero;
        melee.AniCon.Play("EnhancableMelee_Idle");
    }

    public override void Update(EnhancableMelee melee)
    {
        timer += Time.deltaTime;
        if(timer >= duration)
        {
            if(melee.Target != null)
            {
                if(melee.DistanceToPlayer() <= melee.Stat.moveDistance)
                {
                    melee.ChangeState(melee.State["Attack"]);
                }
                else
                {
                    melee.ChangeState(melee.State["Chase"]);
                }
            }
            else
            {
                melee.ChangeState(melee.State["Idle"]);
            }
        }
    }

    public override void Exit(EnhancableMelee melee)
    {

    }
}

public class EMDestroyState : EnhancableMeleeState
{
    LayerMask origin;
    public override void Start(EnhancableMelee melee)
    {
        melee.StopAttack();
        melee.Rigid.linearVelocity = Vector2.zero;
        origin = melee.gameObject.layer;
        melee.gameObject.layer = LayerMask.NameToLayer("Invincible");
    }

    public override void Update(EnhancableMelee melee)
    {

    }

    public override void Exit(EnhancableMelee melee)
    {
        melee.gameObject.layer = origin;
    }
}

