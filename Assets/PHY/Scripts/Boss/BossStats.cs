using UnityEngine;

/// <summary>
/// 2025-11-24 PHY
/// 보스 피를 2000으로 임시로 지정해놓고 왜 특정 체력이 되면 터지는지 내 코드가 잘못된건지
/// 머리 쥐어 싸매고 있었는데 BossAI에 있는 페이즈체크함수에 60퍼 이하면 2페이즈 패턴으로 넘어가도록 되어있는데
/// 아직 2페이즈 패턴이 설계가 되어있지 않아서 터지는 것이었다. 
/// </summary>

public class BossStats : MonoBehaviour
{
    private BossAI bossAI;
    [Header("보스 체력 관련 변수")]
    public float maxHP = 5000;
    public float currentHP;

    // 보스 공격력이 필요한지는 모르겠지만 일단 만들었음
    [Header("보스 패턴 딜(?) 관련 변수")]
    private float baseAttackDamage = 20f;

    [Header("보스 페이즈 전환 관련 변수")]
    private bool isInvicible = false;       // 페이즈 바뀔 때 잠시 무적처리 하는 변수
    private bool isNextPhase = false;     // 다음 페이즈로 넘어가는 중인지 확인하는 변수

    [Header("보스 죽음 관련 변수")]
    private bool isDead = false;

    private void Awake()
    {
        bossAI = GetComponent<BossAI>();
    }

    private void Start()
    {
        currentHP = maxHP;
    }

    // 보스 데미지 입는 함수
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        CheckPhase();

        if (currentHP <= 0)
        {
            Dead();
        }
    }

    /// <summary>
    /// 2025-11-25 PHY
    /// 플레이어가 스킬을 쓰면 보스가 받는 딜을 어떻게 해야하지
    /// TakeDamage에서 같이 처리해야하는건가?
    /// 아님 따로 스킬 딜 들어오는 관련 함수를 만들어서 처리를 해야할까?
    /// 스킬 딜이 얼마나 될진 모르겠는데 혹시나 보스 전체 체력의 절반을 날리는 그런 스킬이 있으면
    /// 페이즈 전환도 고려해야하는데 큰일났네 이거 우짜노;;;;
    /// </summary>
    /// 

    // 플레이어가 스킬을 쓸 때 보스가 받는 딜 관련 함수
    public float AttackedDamage()
    {
        // 근데 이게 필요한가 싶은 생각이 듦.. 필요한가? 
        // skill.GetSkillDamage() <- 이런 함수로 받아와야하나?
        return 0.0f;
    }

    public void CheckPhase()
    {
        float hpRatio = currentHP / maxHP;

        int phaseIndex = -1;    // 기본 설정 및 페이즈 변환 없음


        if (hpRatio <= 0.6f && bossAI.CurrentPhase == bossAI.AllPhase[0])
        {
            phaseIndex = 1;         // 10% 이하 광폭
        }
        else if (hpRatio <= 0.1f && bossAI.CurrentPhase == bossAI.AllPhase[1])
        {
            phaseIndex = 2;         // 60% 이하 2페이즈
        }

        if (phaseIndex == -1) return;

        // Todo
        // 발생한 문제 : Trigger로 처리하면 연사가 되지만 딜이 동시에 많이 들어가 60% 이하로 떨어질 때 2페이즈로 넘어가는 인덱스 에러가
        // 나와야하는데 피가 절반 깎여야 2페이즈로 넘어가는 문제가 발생함. tlqkf 
        // 2025-11-26 해결 완료

        // 페이즈 중복 전환 방지를 위한 방어 코드

        if (bossAI.CurrentPhase != bossAI.AllPhase[phaseIndex])
        {
            bossAI.SetPhase(phaseIndex);
            Debug.Log("보스 페이즈 전환 : " + phaseIndex);
        }
    }

    // 딜레이(경직)도 일단 보류
    public void Delay()
    {
        if (isDead)
        {
            // Todo
            // Delay 로직 넣기
            // 어떻게 넣어야할까? 1초 경직을 하려면 코루틴에 있는 WaitForSeconds를 써야하나?
            // 그건 아닌거 같은데 음....
            // 일단 스탯에서 해야할 건 경직신호를 주는 신호기 만들기
        }
    }


    public void Dead()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Boss dead");

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))     // 레이어 이름으로 비교
        {
            Debug.Log("Boss taked TakeDamage");

            TakeDamage(25.0f);       // 임시 딜 

            Debug.Log("Boss HP : " + currentHP);

            return;

        }
    }

}
