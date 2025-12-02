using System.Collections;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.XR;

public abstract class PlayerState
{
    public abstract void Start(Player player);
    public abstract void Update(Player player);
    public abstract void Exit(Player player);
}

public class PlayerIdleState : PlayerState
{
    public override void Start(Player player)
    {
        // 모든 공격 상태 해제
        // 팔 비활성화
        
    }

    public override void Update(Player player)
    {
        
    }

    public override void Exit(Player player)
    {
        
    }
}

public class PlayerMeleeAttackState : PlayerState
{
    float timer;

    public override void Start(Player player)
    {
        timer = 0;
    }

    public override void Update(Player player)
    {
        if(player.Attacking)
        {
            timer = 0;
        }

        timer += Time.deltaTime;
        if (timer >= player.Stats.MeleeAttackRelaxTime)
        {
            player.ChangeState(player.States["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        player.MeleeAttackIndex = 0;
    }
}

public class PlayerRangeAttackState : PlayerState
{
    // 사격 상태

    float timer;

    public override void Start(Player player)
    {
        // 진정 타이머 초기화
        // 사격 상태 활성화
        // 사격 팔 활성화
        timer = 0;
        player.Aiming = true;
        player.Arm.EnableSprite(true);
    }

    public override void Update(Player player)
    {
        // 진정 타이머 초과 전까지 상태 유지
        // 입력 없으면 진정 타이머 갱신
        player.RotateArm();
        timer += Time.deltaTime;
        if (timer >= player.Stats.RangeAttackRelaxTime)
        {
            player.ChangeState(player.States["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        player.Aiming = false;
        player.Arm.EnableSprite(false);
    }
}

public class PlayerParryingState : PlayerState
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

public class PlayerDodgeState : PlayerState
{
    // 회피 상태
    float currentVel;
    float gravityScale;
    LayerMask maskOrigin;

    public override void Start(Player player)
    {
        // 중력 계수 저장 및 제거 
        // Y축 속도 제거
        maskOrigin = player.gameObject.layer;
        player.gameObject.layer = LayerMask.NameToLayer("Invincible");
        gravityScale = player.Rigid.gravityScale;
        player.Dodging = true;
        player.Rigid.gravityScale = 0f;
        player.Rigid.linearVelocityY = 0f;
        player.GhTr.gameObject.SetActive(true);
    }

    public override void Update(Player player)
    {
        // Y축 속도 제거
        player.Rigid.linearVelocityY = 0f;
        
        // 속도 변화
        currentVel = player.Rigid.linearVelocityX;
        currentVel = Mathf.Lerp(currentVel, 0, 5*Time.deltaTime);
        player.Rigid.linearVelocityX = currentVel;

        // 자연스러운 상태 복귀(극한점 튜닝)
        if(Mathf.Abs(currentVel) <= player.Stats.returnVelocity)
        {
            player.Rigid.gravityScale = gravityScale;
            player.ChangeState(player.States["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        // 회피 상태 해제
        // 중력 계수 복귀
        player.gameObject.layer = maskOrigin;
        player.Dodging = false;
        player.Rigid.gravityScale = gravityScale;
        player.GhTr.gameObject.SetActive(false);
    }
}

public class PlayerClimbState : PlayerState
{
    float gravityScale;
    public override void Start(Player player)
    {
        player.Check.OnWall = true;
        player.Check.CanWallJump = true;
        gravityScale = player.Rigid.gravityScale;
        player.Rigid.linearVelocityY = 0f;
        player.Rigid.gravityScale = 0f;
    }

    public override void Update(Player player)
    {
        if(player.Check.IsGround)
        {
            player.ChangeState(player.States["Idle"]);
        }

        player.Rigid.linearVelocityY = -1;
    }

    public override void Exit(Player player)
    {
        player.Check.OnWall = false;
        player.Check.CanWallJump = false;
        player.Rigid.gravityScale = gravityScale;
    }
}

public class PlayerWallJumpState : PlayerState
{
    float timer;

    public override void Start(Player player)
    {
        timer = 0;

    }

    public override void Update(Player player)
    {
        timer += Time.deltaTime;
        if(timer > player.Stats.wallJumpDuration)
        {
            player.ChangeState(player.States["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        
    }

}

public class PlayerHitState : PlayerState
{
    float timer;

    public override void Start(Player player)
    {
        timer = 0;
        player._Hit = true;
        player.Rigid.linearVelocity = Vector2.zero;
        player.KnockBack();
        player.StartCoroutine(InvincibleTime(player));
    }

    public override void Update(Player player)
    {
        timer += Time.deltaTime;
        if(timer > player.Stats.hitLimit)
        {
            player.ChangeState(player.States["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        player._Hit = false;
        player.Rigid.linearVelocity = Vector2.zero;
    }

    IEnumerator InvincibleTime(Player player)
    {
        LayerMask originMask = player.gameObject.layer;

        player.gameObject.layer = LayerMask.NameToLayer("Invincible");

        yield return CoroutineCasher.Wait(player.Stats.invincibleTime);

        player.gameObject.layer = originMask;
    }
}
