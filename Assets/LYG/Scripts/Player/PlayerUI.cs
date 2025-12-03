using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] Image targetCrossHairPrefab;
    [SerializeField] GameObject meleeSkills;
    [SerializeField] GameObject rangeSkills;
    [SerializeField] Image chargeAttackIcon;
    [SerializeField] Image flashAttackIcon;
    [SerializeField] Image phantomBladeIcon;
    [SerializeField] Image autoTargetingIcon;
    [SerializeField] Image stamina;
    [SerializeField] Text bulletCount;

    public Image playerCrossHair;
    public List<Image> targetCrossHair;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        for(int i = 0; i< 10; i++)
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
        switch(player.CrWp)
        {
            case WeaponState.Melee:
                if (rangeSkills.activeSelf)
                    rangeSkills.SetActive(false);
                meleeSkills.SetActive(true);
                chargeAttackIcon.fillAmount = player.SkMn.ChargeAttackTimer / player.SkMn.ChargeAttackStat.coolTime;
                flashAttackIcon.fillAmount = player.SkMn.FlashAttackTimer / player.SkMn.FlashAttackStat.coolTime;
                break;
            case WeaponState.Ranged:
                if (meleeSkills.activeSelf)
                    meleeSkills.SetActive(false);
                rangeSkills.SetActive(true);
                phantomBladeIcon.fillAmount = player.SkMn.PhantomBladeTimer / player.SkMn.PhantomBladeStat.coolTime;
                autoTargetingIcon.fillAmount = player.SkMn.AutoTargetingTimer / player.SkMn.AutoTargetingStat.coolTime;
                break;
        }
    }

}
