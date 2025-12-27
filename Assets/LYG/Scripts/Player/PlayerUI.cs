using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] Canvas canvas;
    [SerializeField] Image targetCrossHairPrefab;
    //[SerializeField] GameObject skills;
    //[SerializeField] GameObject meleeSkills;
    //[SerializeField] GameObject rangeSkills;
    [SerializeField] Image chargeAttackIcon;
    [SerializeField] Image flashAttackIcon;
    [SerializeField] Image phantomBladeIcon;
    [SerializeField] Image autoTargetingIcon;
    [SerializeField] Image stamina;
    [SerializeField] Image overFlowEnergy;
    [SerializeField] Image lifeIcon;
    [SerializeField] Text bulletCount;

    public Image playerCrossHair;
    public List<Image> targetCrossHair;
    public Canvas Cnvs { get => canvas; set => canvas = value; }


    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
        canvas = GetComponent<Canvas>();
        for(int i = 0; i< 12; i++)
        {
            Image instance = Instantiate(targetCrossHairPrefab, transform);
            targetCrossHair.Add(instance);
            targetCrossHair[i].gameObject.SetActive(false);
        }
        
    }

    private void Update()
    {
        SkillUI();
    }

    void SkillUI()
    {
        bulletCount.text = player.BulletCount.ToString();
        stamina.fillAmount = player.Stamina / player.Stats.maxStamina;
        overFlowEnergy.fillAmount = player.OverFlowEnergy / player.Stats.maxOverFlowEnergy;
        //switch(player.CrWp)
        //{
        //    case WeaponState.Melee:
        //        if (rangeSkills.activeSelf)
        //            rangeSkills.SetActive(false);
        //        meleeSkills.SetActive(true);
        //        chargeAttackIcon.fillAmount = player.SkMn.ChargeAttackTimer / player.SkMn.ChargeAttackStat.coolTime;
        //        ChargeAttackUI();
        //        flashAttackIcon.fillAmount = player.SkMn.FlashAttackTimer / player.SkMn.FlashAttackStat.coolTime;
        //        break;
        //    case WeaponState.Ranged:
        //        if (meleeSkills.activeSelf)
        //            meleeSkills.SetActive(false);
        //        rangeSkills.SetActive(true);
        //        phantomBladeIcon.fillAmount = player.SkMn.PhantomBladeTimer / player.SkMn.PhantomBladeStat.coolTime;
        //        autoTargetingIcon.fillAmount = player.SkMn.AutoTargetingTimer / player.SkMn.AutoTargetingStat.coolTime;
        //        break;
        //}
        PhantomBladeUI();
        ChargeAttackUI();
        FlashAttackUI();
        AutoTargetingUI();
    }

    void PhantomBladeUI()
    {
        if(player.SkMn.PhantomBladeUsable)
        {
            phantomBladeIcon.transform.SetAsLastSibling();
            phantomBladeIcon.gameObject.SetActive(false);
        }
        else
        {
            phantomBladeIcon.gameObject.SetActive(true);
            phantomBladeIcon.fillAmount = player.SkMn.PhantomBladeTimer / player.SkMn.PhantomBladeStat.coolTime;
        }
    }

    void ChargeAttackUI()
    {
        if(player.SkMn.ChargeAttackUsable)
        {
            Debug.Log("비활성화");
            chargeAttackIcon.transform.SetAsLastSibling();
            chargeAttackIcon.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("활성화");

            chargeAttackIcon.gameObject.SetActive(true);
            chargeAttackIcon.fillAmount = player.SkMn.ChargeAttackTimer / player.SkMn.ChargeAttackStat.coolTime;
        }
    }

    void FlashAttackUI()
    {
        if (player.SkMn.FlashAttackUsable)
        {
            Debug.Log("비활성화");
            flashAttackIcon.transform.SetAsLastSibling();
            flashAttackIcon.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("활성화");

            flashAttackIcon.gameObject.SetActive(true);
            flashAttackIcon.fillAmount = player.SkMn.FlashAttackTimer / player.SkMn.FlashAttackStat.coolTime;
        }
    }

    void AutoTargetingUI()
    {
        if (player.SkMn.AutoTargetingUsable)
        {
            Debug.Log("비활성화");
            autoTargetingIcon.transform.SetAsLastSibling();
            autoTargetingIcon.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("활성화");

            autoTargetingIcon.gameObject.SetActive(true);
            autoTargetingIcon.fillAmount = player.SkMn.AutoTargetingTimer / player.SkMn.AutoTargetingStat.coolTime;
        }
    }

    void LifeUI()
    {
       
    }

}
