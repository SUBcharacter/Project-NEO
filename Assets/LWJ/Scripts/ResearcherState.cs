//using UnityEngine;
//using UnityEngine.EventSystems;
//using static UnityEngine.RuleTile.TilingRuleOutput;
//
//public abstract class ResearcherState 
//{
//    public abstract void Start(Researcher researcher);
//    public abstract void Update(Researcher researcher);
//    public abstract void Exit(Researcher researcher);
//}
//
//public class R_IdleState : ResearcherState
//{
//    public override void Start(Researcher researcher)
//    {
//        Debug.Log("Researcher Idle State 시작");
//    }
//    public override void Update(Researcher researcher)
//    {
//
//        if (researcher.isDroneSummoned == false)
//        {
//            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
//            {
//                Debug.Log("플레이어 감지!");
//                researcher.ChangeState(researcher.R_States[2]);
//                return;
//            }     
//        }
//        else
//        {
//            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
//            {         
//                researcher.ChangeState(researcher.R_States[5]);
//                return;
//            }
//        }      
//        researcher.rigid.linearVelocity = Vector2.zero;
//    }
//
//
//    public override void Exit(Researcher researcher)
//    {
//        Debug.Log("Researcher Idle State 종료");
//    }
//
//}
//
//public class R_WalkState : ResearcherState
//{
//    public override void Start(Researcher researcher)
//    {
//        Debug.Log("Researcher Walk State 시작");
//        researcher.animator.Play("R_Move");
//    }
//    public override void Update(Researcher researcher)
//    {
//
//        if (researcher.isDroneSummoned == false)
//        {
//            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight )
//            {
//                Debug.Log("플레이어 감지!");
//                researcher.ChangeState(researcher.R_States[2]);
//                return;
//            }
//        }
//        else
//        {
//            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
//            {
//                Debug.Log("플레이어 추적 상태 진입!");
//                researcher.ChangeState(researcher.R_States[5]);
//                return;
//            }
//        }
//        researcher.WallorLedgeFlip(researcher);
//
//    }
//
//
//    public override void Exit(Researcher researcher)
//    {
//        Debug.Log("Researcher walk State 종료");
//
//        researcher.rigid.linearVelocity = Vector2.zero;
//    }
//
//}
//
//public class R_SummonDroneState : ResearcherState
//{
//    public override void Start(Researcher researcher)
//    {
//        Debug.Log("드론 소환");
//        researcher.rigid.linearVelocity = Vector2.zero;
//        researcher.SummonDrone();
//        researcher.animator.Play("R_Summon");
//    }
//    public override void Update(Researcher researcher)
//    {
//
//    }
//    public override void Exit(Researcher researcher)
//    {
//        Debug.Log("Summon Drone State 종료");
//        
//    }
//}
//
//public class  R_ChaseState : ResearcherState
//{
//    public override void Start(Researcher researcher)
//    {
//        Debug.Log("추적");
//        researcher.animator.Play("R_Move");
//    }
//    public override void Update(Researcher researcher)
//    {
//
//        if (researcher.CheckForObstacle(researcher) || researcher.CheckForLedge(researcher))
//        {
//            researcher.ChangeState(researcher.R_States[1]);
//            return;
//        }
//        researcher.Chase();
//
//        if(researcher.aimRange != null && researcher.aimRange.IsPlayerInSight)
//        {
//            researcher.ChangeState(researcher.R_States[3]);
//            return;
//        }   
//
//    }
//    public override void Exit(Researcher researcher)
//    {
//        Debug.Log("추적 상태 종료");
//       
//    }
//}
//public class R_Attackstate : ResearcherState
//{
//    private bool active;
//    public override void Start(Researcher researcher)
//    {
//        active = true;
//        Debug.Log("연구원 공격!");
//
//        researcher.Armsetactive(active);
//        researcher.animator.Play("R_attack");
//        researcher.rigid.linearVelocity = Vector2.zero;
//    }
//    public override void Update(Researcher researcher)
//    {
//        researcher.Attack();
//    }
//    public override void Exit(Researcher researcher)
//    {
//        active = false;
//        Debug.Log("Researcher Attack State 종료");
//        researcher.Armsetactive(active);
//        researcher.isarmlock = false;
//    }
// 
//}
//
//public class R_Hitstate : ResearcherState
//{
//    private float hitDuration = 0.1f; // 넉백 지속 시간
//    private float exitTime;
//    float directionToPlayer;
//    public override void Start(Researcher researcher)
//    {
//
//        Debug.Log("Researcher Hit State 시작");
//        exitTime = Time.time + hitDuration;
//        directionToPlayer = researcher.Player_Trans.position.x - researcher.transform.position.x;
//        researcher.Knockback();
//    }
//    public override void Update(Researcher researcher)
//    {
//        if (Time.time >= exitTime)
//        {
//            researcher.FlipResearcher(researcher, directionToPlayer);
//            researcher.rigid.linearVelocity = Vector2.zero;
//            if (researcher.sightRange != null && researcher.sightRange.IsPlayerInSight)
//            {
//
//                if (researcher.aimRange != null && researcher.aimRange.IsPlayerInSight)
//                { 
//                    researcher.ChangeState(researcher.R_States[3]); 
//                }
//                else
//                {               
//                    researcher.ChangeState(researcher.R_States[5]); 
//                }
//            }
//            else 
//            {
//                researcher.ChangeState(researcher.R_States[1]);
//            }
//
//        }
//    }
//    public override void Exit(Researcher researcher)
//    {
//        Debug.Log("Researcher Hit State 종료");
//    }
//}
//
//public class R_Deadstate : ResearcherState
//{
//
//    public override void Start(Researcher researcher)
//    {
//
//        Debug.Log("Researcher Dead State 시작");
//        researcher.rigid.linearVelocity = Vector2.zero;
//        researcher.animator.Play("R_Death");
//
//        
//    }
//    public override void Update(Researcher researcher)
//    {
//
//    }
//    public override void Exit(Researcher researcher)
//    {
//        Debug.Log("Researcher Dead State 종료");
//    }
//}
//