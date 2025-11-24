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
    private float attackDamage = 20f;

    [Header("보스 딜레이 관련 변수")]
    private bool isHit = false;

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
    public void Damage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        CheckPhase();

        if (currentHP <= 0)
        {
            Dead();
        }
    }
    public void CheckPhase()
    {
        float hpRatio = currentHP / maxHP;

        int phaseIndex = -1;    // 기본 설정 및 페이즈 변환 없음

        if (hpRatio <= 0.1f)
            phaseIndex = 2;         // 10% 이하 광폭
        else if (hpRatio <= 0.6f)
            phaseIndex = 1;         // 60% 이하 2페이즈

        if (phaseIndex == -1) return;

        // 페이즈 중복 전환 방지를 위한 방어 코드
        if (bossAI.CurrentPhase != bossAI.allPhases[phaseIndex])
        {
            bossAI.SetPhase(phaseIndex);
            Debug.Log("보스 페이즈 전환 : " + phaseIndex);
        }
    }

    public void Delay()
    {
        if (isHit)
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
        if (collision.gameObject.name.Contains("PlayerBullet"))     // 프리팹 이름으로 보스 데미지 입히기 (임시방편임)
        {
            Debug.Log("Boss taked Damage");

            Damage(25.0f);       // 임시 딜 

            Debug.Log("Boss HP : " + currentHP);

            return;

        }
    }
}
