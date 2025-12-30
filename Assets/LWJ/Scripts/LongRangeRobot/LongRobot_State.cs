using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class LongRobot_State 
{
    public abstract void Start(LongRobot rb);
    public abstract void Update(LongRobot rb);
    public abstract void Exit(LongRobot rb);
}

public class LRB_Idlestate : LongRobot_State
{
    float idleDuration = 0f;
    float waitTime = 1f;
    public override void Start(LongRobot rb)
    {
        Debug.Log("로봇 Idle State 시작");
        rb.Rigid.linearVelocity = Vector2.zero;
        rb.animator.Play("LRB_Idle");
    }
    public override void Update(LongRobot rb)
    {
        idleDuration += Time.deltaTime;

        if (rb.sightrange.PlayerInSight != null)
        {
            rb.ChangeState(rb.states[EnemyTypeState.Chase]);
            return;
        }

        if (idleDuration >= waitTime)
        {
            float randomDir = Random.value > 0.5f ? 1f : -1f;
            rb.FlipRobot(randomDir);
            rb.ChangeState(rb.states[EnemyTypeState.Walk]);
            idleDuration = 0f;
        }

 
    }
    public override void Exit(LongRobot rb) { }
}

public class LRB_Walkstate : LongRobot_State
{
    private Vector2 startpos;
    private float movedistance = 2.0f;
    public override void Start(LongRobot rb)
    {
        startpos = rb.transform.position;
        Debug.Log("로봇 Walk State 시작");
        rb.animator.Play("LRB_Walk");
    }
    public override void Update(LongRobot rb)
    {
        if (rb.CheckForObstacle())
        {
            rb.ChangeState(rb.states[EnemyTypeState.Idle]);
            return;
        }
        if (rb.sightrange.PlayerInSight != null)
        {
            rb.ChangeState(rb.states[EnemyTypeState.Chase]);
            return;
        }

        float movedDistance = Vector2.Distance(startpos, rb.transform.position);
        if (movedDistance >= movedistance)
        {
            rb.ChangeState(rb.states[EnemyTypeState.Idle]);
            return;
        }
        rb.Move();
    }
    public override void Exit(LongRobot rb) { }
}

public class LRB_Attackstate : LongRobot_State
{
    public override void Start(LongRobot rb)
    {
        Debug.Log("Attack State 시작");
        rb.Rigid.linearVelocity = Vector2.zero;
        rb.target = rb.Pl_trans.position;
        rb.animator.Play("LRB_Attack", -1, 0f);
    }
    public override void Update(LongRobot rb)
    {
        rb.Attack();
    }
    public override void Exit(LongRobot rb) { }
}

public class LRB_Deadstate : LongRobot_State
{
    LayerMask origin;
    public override void Start(LongRobot rb)
    {
        Debug.Log("Dead State 시작");
        rb.Rigid.linearVelocity = Vector2.zero;
        origin = rb.gameObject.layer;
        rb.gameObject.layer = LayerMask.NameToLayer("Invincible");
        rb.animator.Play("LRB_Death");
    }
    public override void Update(LongRobot rb) { }
    public override void Exit(LongRobot rb) 
    {
        rb.gameObject.layer = origin;
    }
}

public class LRB_Hitstate : LongRobot_State
{
    private float hitDuration = 0.1f;
    private float exitTime;
    public override void Start(LongRobot rb)
    {
        Debug.Log("Hit State 시작");
        rb.Rigid.linearVelocity = Vector2.zero;
        exitTime = hitDuration + Time.time;
        rb.animator.Play("LRB_Hit");
    }
    public override void Update(LongRobot rb)
    {
        if (Time.time >= exitTime)
        {
            if(rb.Pl_trans != null)
            {
                float direction = rb.Pl_trans.position.x - rb.transform.position.x;
                rb.FlipRobot(direction);
                if (rb.DistanceToPlayer() <= rb.Stat.moveDistance)
                {
                    rb.ChangeState(rb.states[EnemyTypeState.Attack]);
                }
                else
                {
                    rb.ChangeState(rb.states[EnemyTypeState.Chase]);
                }
            }
        }

    }
    public override void Exit(LongRobot rb) { }
}

public class LRB_EnhancedState : LongRobot_State
{
    LayerMask origin;
    public override void Start(LongRobot rb)
    {
        Debug.Log("Enhanced State 시작");
        rb.Rigid.linearVelocity = Vector2.zero;
        origin = rb.gameObject.layer;
        rb.gameObject.layer = LayerMask.NameToLayer("Invincible");
        rb.animator.Play("LRB_Enhance");
    }
    public override void Update(LongRobot rb)
    {
        if (rb.Enhanced)
        {
            if (rb.DistanceToPlayer() <= rb.Stat.moveDistance)
            {
                rb.ChangeState(rb.states[EnemyTypeState.Attack]);
            }
            else
            {
                rb.ChangeState(rb.states[EnemyTypeState.Chase]);
            }
        }
    }
    public override void Exit(LongRobot rb) 
    { 
        rb.gameObject.layer = origin;
    }
}

public class LRB_Chasestate : LongRobot_State
{
    public override void Start(LongRobot rb)
    {
        Debug.Log("Chase State 시작");
        rb.animator.Play("LRB_Walk");
    }
    public override void Update(LongRobot rb)
    {
        if (rb.CheckForObstacle())
        {
            rb.ChangeState(rb.states[EnemyTypeState.Idle]);
            return;
        }

        if (rb.DistanceToPlayer() <= rb.Stat.moveDistance)
        {
            rb.ChangeState(rb.states[EnemyTypeState.Attack]);
            return;
        }
        rb.Chase();
    }
    public override void Exit(LongRobot rb) { }
}