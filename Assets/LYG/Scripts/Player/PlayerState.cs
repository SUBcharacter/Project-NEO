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
    // 근접 공격 상태 - 제작 중
    float timer;

    public override void Start(Player player)
    {
        timer = 0;
    }

    public override void Update(Player player)
    {
        if(player.attacking)
        {
            timer = 0;
        }

        timer += Time.deltaTime;
        if (timer >= player.stats.MeleeAttackRelaxTime)
        {
            player.ChangeState(player.states["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        player.meleeAttackIndex = 0;
    }
}

public class PlayerRangeAttackState : PlayerState
{
    // 사격 상태

    float timer;
    float relaxTime = 3f;

    public override void Start(Player player)
    {
        // 진정 타이머 초기화
        // 사격 상태 활성화
        // 사격 팔 활성화
        timer = 0;
        player.aiming = true;
        player.arm.gameObject.SetActive(true);
    }

    public override void Update(Player player)
    {
        // 진정 타이머 초과 전까지 상태 유지
        // 입력 없으면 진정 타이머 갱신
        player.RotateArm();
        timer += Time.deltaTime;
        if (timer >= player.stats.RangeAttackRelaxTime)
        {
            player.ChangeState(player.states["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        player.aiming = false;
        player.arm.gameObject.SetActive(false);
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
        gravityScale = player.rigid.gravityScale;
        player.dodging = true;
        player.rigid.gravityScale = 0f;
        player.rigid.linearVelocityY = 0f;
        player.ghostTrail.gameObject.SetActive(true);
    }

    public override void Update(Player player)
    {
        // Y축 속도 제거
        player.rigid.linearVelocityY = 0f;
        
        // 속도 변화
        currentVel = player.rigid.linearVelocityX;
        currentVel = Mathf.Lerp(currentVel, 0, 5*Time.deltaTime);
        player.rigid.linearVelocityX = currentVel;

        // 자연스러운 상태 복귀(극한점 튜닝)
        if(Mathf.Abs(currentVel) <= player.stats.returnVelocity)
        {
            player.rigid.gravityScale = gravityScale;
            player.ChangeState(player.states["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        // 회피 상태 해제
        // 중력 계수 복귀
        player.gameObject.layer = maskOrigin;
        player.dodging = false;
        player.rigid.gravityScale = gravityScale;
        player.ghostTrail.gameObject.SetActive(false);
    }
}

public class PlayerClimbState : PlayerState
{
    float gravityScale;
    public override void Start(Player player)
    {
        player.onWall = true;
        player.canWallJump = true;
        gravityScale = player.rigid.gravityScale;
        player.rigid.linearVelocityY = 0f;
        player.rigid.gravityScale = 0f;
    }

    public override void Update(Player player)
    {
        if(player.isGround)
        {
            player.ChangeState(player.states["Idle"]);
        }

        player.rigid.linearVelocityY = -1;
    }

    public override void Exit(Player player)
    {
        player.onWall = false;
        player.canWallJump = false;
        player.rigid.gravityScale = gravityScale;
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
        if(timer > player.stats.wallJumpDuration)
        {
            player.ChangeState(player.states["Idle"]);
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
        player.hit = true;
        player.rigid.linearVelocity = Vector2.zero;
        player.KnockBack();
        player.StartCoroutine(InvincibleTime(player));
    }

    public override void Update(Player player)
    {
        timer += Time.deltaTime;
        if(timer > player.stats.hitLimit)
        {
            player.ChangeState(player.states["Idle"]);
        }
    }

    public override void Exit(Player player)
    {
        player.hit = false;
        player.rigid.linearVelocity = Vector2.zero;
    }

    IEnumerator InvincibleTime(Player player)
    {
        LayerMask originMask = player.gameObject.layer;

        player.gameObject.layer = LayerMask.NameToLayer("Invincible");

        yield return CoroutineCasher.Wait(player.stats.invincibleTime);

        player.gameObject.layer = originMask;
    }
}
