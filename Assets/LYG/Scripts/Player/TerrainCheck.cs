using Unity.Android.Gradle;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class TerrainCheck : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] Vector2 groundNormal;

    [SerializeField] bool canAirJump;
    [SerializeField] bool canDodge;
    [SerializeField] bool wallLeft;
    [SerializeField] bool wallRight;
    [SerializeField] bool jumped;
    [SerializeField] bool onWall;
    [SerializeField] bool canWallJump;
    [SerializeField] bool isGround;
    [SerializeField] bool onSlope;


    public Vector2 GroundNormal => groundNormal;
    public bool WallLeft => wallLeft;
    public bool WallRight => wallRight;
    public bool OnSlope => onSlope;
    public bool IsGround => isGround;
    public bool CanAirJump { get => canAirJump; set => canAirJump = value; }
    public bool OnWall { get => onWall; set => onWall = value; }
    public bool CanWallJump { get => canWallJump; set => canWallJump = value; }
    public bool Jumped { get => jumped; set => jumped = value; }
    public bool CanDodge { get => canDodge; set => canDodge = value; }

    //[SerializeField] Vector2 groundNormal;
    //float slopeAngle;
    //float slopeLostTimer;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        GroundCheck();
        WallCheck();
    }


    void GroundCheck()
    {
        if (player.CrSt is PlayerHitState)
            return;
        // 지형 체크

        // 캡슐 콜라이더의 하단 반원부분 중심위치 계산
        float radius = player.Col.size.x / 2f;
        float bottomY = -player.Col.size.y / 2f + radius; // 범위 튜닝

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        // 서클캐스트(Gizmo 옵션 적용)       
        RaycastHit2D hit = Physics2D.CircleCast(bottomCenter, radius, Vector2.down, 0.05f, player.Stats.groundMask);

        if (hit.collider != null)
        {
            Vector2 nor = hit.normal;

            bool isVerticalWall = Mathf.Abs(nor.x) >= 1f && Mathf.Abs(nor.y) <= 0f;

            if (isVerticalWall)
            {
                isGround = false;
            }
            else
            {
                isGround = true;
            }
        }
        else
        {
            isGround = false;
        }

        // 착지 상태 일시 초기화
        if (isGround)
        {
            canAirJump = isGround;
            canDodge = isGround;
            jumped = !isGround;
            onWall = false;
        }
    }

    //// 경사면 레벨 디자인 시 활용
    //public void SlopeCheck()
    //{
    //    if (player.CrSt is PlayerHitState || player.SkMn.charging)
    //        return;
    //    // 경사면 물리 연산
    //    // 수직, 수평을 레이캐스트로 검사 해서, 법선 벡터와, 경사각을 구해 Move함수에서 적용
    //
    //    // 발사 위치는 GroundCheck의 Origin
    //
    //    float radius = player.Col.size.x / 2f;
    //    float bottomY = -player.Col.size.y / 2f + radius - 0.2f;
    //
    //    Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);
    //
    //    HorizontalSlopeCheck(bottomCenter);
    //    if (!onSlope)
    //        return;
    //    VerticalSlopeCheck(bottomCenter);
    //}
    //
    //void VerticalSlopeCheck(Vector2 bottomCenter)
    //{
    //    // 수직 검사
    //    // 레이캐스트 지속 검사로 인한 연산부담 완화
    //    if(!isGround)
    //    {
    //        onSlope = false;
    //        slopeAngle = 0f;
    //        groundNormal = Vector2.zero;
    //        return;
    //    }
    //
    //    // 아래 방향으로 slopeRayLength(1.5f) 만큼 레이캐스트
    //    RaycastHit2D hit = Physics2D.Raycast(bottomCenter, Vector2.down, player.stats.slopeRayLength, player.stats.groundMask);
    //    Debug.DrawRay(bottomCenter, Vector2.down * player.stats.slopeRayLength, Color.green);
    //    if (hit)
    //    {
    //        // 히트시 해당 지형의 법선 벡터 저장
    //        // 경사각 저장
    //        // 경사각에 따른 onSlope 값 최신화
    //        // slopeLostTimer -> 해당 경사의 끝에서 튕겨져 나가는 현상 억제(해결 힘듬....)
    //
    //        groundNormal = hit.normal;
    //        slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
    //        onSlope = slopeAngle > 0f && slopeAngle <= player.stats.maxSlopeAngle;
    //        slopeLostTimer = 0;
    //        Debug.DrawRay(hit.point, groundNormal, Color.green);
    //    }
    //    else
    //    {
    //        // 없을시 slopeLostTimer 작동
    //        // 타이머 초과시 onSlope, 경사각, 법선 벡터 초기화
    //        slopeLostTimer += Time.deltaTime;
    //        if(slopeLostTimer < player.stats.slopeLostDuration)
    //        {
    //            onSlope = true;
    //        }
    //        else
    //        {
    //            onSlope = false;
    //            slopeAngle = 0f;
    //            groundNormal = Vector2.zero;
    //        }
    //    }
    //
    //    // 경사 유무에 따른 마찰력 계수(물리 머티리얼 2D 인자)
    //    if(onSlope)
    //    {
    //        player.Rigid.sharedMaterial = player.stats.fullFriction;
    //    }
    //    else
    //    {
    //        player.Rigid.sharedMaterial = player.stats.noFriction;
    //    }
    //}
    //
    //void HorizontalSlopeCheck(Vector2 bottomCenter)
    //{
    //    // 수평 검사 - 좌, 우 동시 검사
    //    // 연산 부담 완화
    //    if (!isGround)
    //        return;
    //
    //    // 좌, 우로 레이캐스트 발사
    //    RaycastHit2D leftHit = Physics2D.Raycast(bottomCenter, Vector2.left, player.stats.slopeRayLength, player.stats.groundMask);
    //    RaycastHit2D rightHit = Physics2D.Raycast(bottomCenter, Vector2.right, player.stats.slopeRayLength, player.stats.groundMask);
    //
    //    Debug.DrawRay(bottomCenter, Vector2.left * player.stats.slopeRayLength, Color.magenta);
    //    Debug.DrawRay(bottomCenter, Vector2.right * player.stats.slopeRayLength, Color.green);
    //    // 어느쪽을 맞든 상관 없이 똑같이 최신화
    //    // 법선 벡터, 경사각 최신화
    //    if (leftHit)
    //    {
    //        groundNormal = leftHit.normal;
    //        slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
    //        onSlope = true;
    //        Debug.DrawRay(leftHit.point, groundNormal, Color.blue);
    //    }
    //    else if (rightHit)
    //    {
    //        groundNormal = rightHit.normal;
    //        slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
    //        onSlope = true;
    //        Debug.DrawRay(rightHit.point, groundNormal, Color.red);
    //    }
    //    else
    //    {
    //        groundNormal = Vector2.zero;
    //        slopeAngle = 0f;
    //        onSlope = false;
    //    }
    //
    //    if (onSlope)
    //    {
    //        player.Rigid.sharedMaterial = player.stats.fullFriction;
    //    }
    //    else
    //    {
    //        player.Rigid.sharedMaterial = player.stats.noFriction;
    //    }
    //}

    void WallCheck()
    {
        if (player.CrSt is PlayerHitState)
            return;
        if ((player.CrSt is PlayerWallJumpState) || isGround || player.Dodging || player.SkMn.Charging)
            return;

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius - 0.2f;

        Vector2 origin = (Vector2)transform.position + new Vector2(0, bottomY);
        float rayDistance = (col.size.x / 2f) + 0.02f;

        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, rayDistance, player.Stats.groundMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, rayDistance, player.Stats.groundMask);

        Debug.DrawRay(origin, Vector2.right * rayDistance, Color.red);
        Debug.DrawRay(origin, Vector2.left * rayDistance, Color.blue);

        if (hitRight)
        {
            wallLeft = true;
            if (!(player.CrSt is PlayerClimbState))
            {
                canWallJump = true;
                player.ChangeState(player.States["Climb"]);
            }
        }
        else if (hitLeft)
        {
            wallRight = true;
            if (!(player.CrSt is PlayerClimbState))
            {
                canWallJump = true;
                player.ChangeState(player.States["Climb"]);
            }
        }
        else
        {
            wallLeft = false;
            wallRight = false;
            if (player.Aiming || player.CrSt is PlayerMeleeAttackState)
                return;
            if (!(player.CrSt is PlayerIdleState))
            {
                player.ChangeState(player.States["Idle"]);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        // 레이캐스트 시각화
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        float radius = col.size.x / 2f;
        float bottomY = -col.size.y / 2f + radius;

        Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, bottomY);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottomCenter + Vector2.down * 0.05f, radius);
    }
}
