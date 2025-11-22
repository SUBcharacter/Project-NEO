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
        // 모든 공격 상태 해제
        // 팔 비활성화
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
    // 근접 공격 상태 - 제작 중
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
    // 사격 상태

    float timer;
    float RelaxTimer = 3f;

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
    // 회피 상태
    float currentVel;
    float Yvelocity;
    float gravityScale;

    public override void Start(Player player)
    {
        // 중력 계수 저장 및 제거 
        // Y축 속도 제거
        gravityScale = player.rigid.gravityScale;
        Yvelocity = player.rigid.linearVelocityY;
        player.dodging = true;
        player.rigid.gravityScale = 0f;
        player.rigid.linearVelocityY = 0f;
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
        if(Mathf.Abs(currentVel) <= 3f)
        {
            player.rigid.gravityScale = gravityScale;
            player.ChangeState(player.states[0]);
        }
    }

    public override void Exit(Player player)
    {
        // 회피 상태 해제
        // 중력 계수 복귀
        player.dodging = false;
        player.rigid.gravityScale = gravityScale;
    }
}
