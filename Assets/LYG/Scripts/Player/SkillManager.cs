using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] SkillStat phantomBlade;
    [SerializeField] SkillStat chargeAttack;
    [SerializeField] SkillStat autoTargeting;
    [SerializeField] SkillStat flashAttack;
    [SerializeField] HitBox chargeHitBox;
    [SerializeField] HitBox slashHitBox;
    [SerializeField] Player player;
    public Magazine knifePool;

    public float phantomBladeTimer;
    public float chargeAttackTimer;
    public float autoTargetingTimer;
    public float flashAttackTimer;

    public bool casting;
    public bool openFire;
    public bool phantomBladeUsable;
    public bool chargeAttackUsable;
    public bool autoTargetingUsable;
    public bool flashAttackUsable;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        knifePool = GetComponent<Magazine>();
        casting = false;
        openFire = false;
        phantomBladeUsable = true;
        chargeAttackUsable = true;
        autoTargetingUsable = true;
        flashAttackUsable = true;

        phantomBladeTimer = phantomBlade.coolTime;
        chargeAttackTimer = chargeAttack.coolTime;
        autoTargetingTimer = autoTargeting.coolTime;
        flashAttackTimer = flashAttack.coolTime;
    }

    private void Update()
    {
        PhantomBladeCoolTime();
        ChargeAttackCoolTime();
        AutoTargetingCoolTime();
    }

    #region Phantom Blade

    void PhantomBladeCoolTime()
    {
        if (phantomBladeUsable)
            return;
        phantomBladeTimer -= Time.deltaTime;

        if (phantomBladeTimer <= 0)
        {
            phantomBladeUsable = true;
            phantomBladeTimer = phantomBlade.coolTime;
        }
    }

    public void InitiatingPhantomBlade(Transform[] spawnPoint, Vector2 dir)
    {
        if (!phantomBladeUsable)
        {
            Debug.Log("Äð´Ù¿î");
            return;
        }
        phantomBladeUsable = false;
        casting = true;
        StartCoroutine(PhantomBlade(spawnPoint, dir));
    }

    IEnumerator PhantomBlade(Transform[] spawnPoint, Vector2 dir)
    {
        if (player.stamina >= phantomBlade.staminaCost)
        {
            player.stamina -= phantomBlade.staminaCost;
            int index;
            List<int> usedIndex = new();
            GameObject[] knives = new GameObject[phantomBlade.attackCount];
            Vector2[] spawnPoints = new Vector2[spawnPoint.Length];

            for (int i = 0; i < spawnPoint.Length; i++)
            {
                spawnPoints[i] = spawnPoint[i].localPosition;
            }

            for (int i = 0; i < phantomBlade.attackCount; i++)
            {
                while (true)
                {
                    index = UnityEngine.Random.Range(0, spawnPoint.Length);

                    if (!usedIndex.Contains(index))
                        break;
                }

                usedIndex.Add(index);
                knives[i] = knifePool.Fire(dir, spawnPoints[index]);

                yield return CoroutineCasher.Wait(0.2f);
            }

            yield return CoroutineCasher.Wait(0.1f);

            foreach (var k in knives)
            {
                k.GetComponent<Knife>().Shoot(dir);
            }
            casting = false;
        }
        else
        {
            Debug.Log("½ºÅÂ¹Ì³ª ºÎÁ·");
            casting = false;
        }
    }

    #endregion

    #region Charge Attack

    void ChargeAttackCoolTime()
    {
        if (chargeAttackUsable)
            return;
        chargeAttackTimer -= Time.deltaTime;

        if (chargeAttackTimer <= 0)
        {
            chargeAttackUsable = true;
            chargeAttackTimer = chargeAttack.coolTime;
        }
    }

    public void InitiatingChargeAttack(Vector2 dir)
    {
        if (!chargeAttackUsable)
        {
            Debug.Log("Äð´Ù¿î");
            return;
        }
        chargeAttackUsable = false;
        casting = true;
        StartCoroutine(ChargeAttack(dir));
    }

    public IEnumerator ChargeAttack(Vector2 dir)
    {
        if (player.stamina >= chargeAttack.staminaCost)
        {
            player.charging = true;
            player.stamina -= chargeAttack.staminaCost;
            Debug.Log("Chaaarge");
            LayerMask originMask = player.gameObject.layer;
            float gravityScale = player.rigid.gravityScale;
            float velocity = 0;

            player.rigid.gravityScale = 0;
            player.gameObject.layer = LayerMask.NameToLayer("Invincible");
            player.rigid.linearVelocity = Vector2.zero;
            player.col.enabled = false;
            yield return CoroutineCasher.Wait(0.01f);
            chargeHitBox.Init();
            while (true)
            {

                velocity = Mathf.MoveTowards(velocity, chargeAttack.chargeSpeed, chargeAttack.chargeAccel * Time.deltaTime);
                player.rigid.linearVelocity = dir * velocity;

                if (chargeHitBox.triggered)
                {
                    player.rigid.linearVelocity = Vector2.zero;
                    chargeHitBox.gameObject.SetActive(false);

                    RaycastHit2D hit = Physics2D.Raycast(player.transform.position, dir, 5f, chargeHitBox.stats.attackable);
                    Debug.DrawRay(player.transform.position, dir * 5f, Color.red);
                    Vector2 knockDir;
                    if (hit.collider != null)
                    {
                        knockDir = Vector2.Reflect(dir.normalized, hit.normal);
                        Debug.DrawRay(player.transform.position, knockDir * 5f, Color.green);
                    }
                    else
                    {
                        knockDir = -dir;
                    }

                    player.rigid.linearVelocity = knockDir * chargeAttack.knockBackForce;
                    Debug.Log("ºÎµúÇôÀÕ");
                    break;
                }

                if (Mathf.Abs(velocity) >= chargeAttack.chargeSpeed - 1f)
                {
                    player.rigid.linearVelocity = Vector2.zero;
                    chargeHitBox.gameObject.SetActive(false);
                    break;
                }

                yield return null;
            }

            player.rigid.gravityScale = gravityScale;
            player.col.enabled = true;

            yield return CoroutineCasher.Wait(0.2f);

            player.gameObject.layer = originMask;
            player.charging = false;
            casting = false;
        }
        else
        {
            casting = false;
        }
    }

    #endregion

    #region Auto Targeting

    public void AutoTargetingCoolTime()
    {
        if (autoTargetingUsable)
            return;
        autoTargetingTimer -= Time.deltaTime;
        if(autoTargetingTimer <= 0)
        {
            autoTargetingUsable = true;
            autoTargetingTimer = autoTargeting.coolTime;
        }
    }

    public void InitiatingAutoTargeting()
    {
        if(!autoTargetingUsable)
        {
            Debug.Log("Äð´Ù¿î");
            return;
        }

        if(!casting)
        {
            Debug.Log("½ºÄµÁß");
            casting = true;
            StartCoroutine(ScanTargets());
        }
        else
        {
            Debug.Log("ÀÏÁ¦ »ç°Ý");
            openFire = true;
        }
    }

    Collider2D[] Scanning()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(player.transform.position, autoTargeting.scanRadius, autoTargeting.scanable);

        if(targets.Length == 0)
        {
            foreach (var c in player.UI.targetCrossHair)
            {
                c.gameObject.SetActive(false);
            }
            Debug.Log("Å¸°Ù ¾øÀ½");
            return targets;
        }

        System.Array.Sort(targets, (a, b) =>
        Vector2.Distance(player.transform.position,a.transform.position).
        CompareTo(Vector2.Distance(player.transform.position,b.transform.position))
        );

        int count = Mathf.Min(autoTargeting.bulletCost, targets.Length);
        Collider2D[] result = new Collider2D[count];

        Array.Copy(targets, result, count);

        for(int i = 0; i < player.UI.targetCrossHair.Count; i++)
        {
            if(i < count)
            {
                player.UI.targetCrossHair[i].rectTransform.position = Camera.main.WorldToScreenPoint(result[i].transform.position);
                player.UI.targetCrossHair[i].gameObject.SetActive(true);
            }
            else
            {
                player.UI.targetCrossHair[i].gameObject.SetActive(false);
            }
            
            
        }

        Debug.Log(targets.Length);

        return result;
    }

    void OpenFire(Collider2D[] targets)
    {
        if(targets.Length == 0)
        {
            return;
        }
        autoTargetingUsable = false;
        foreach(var t in targets)
        {
            if (t == null)
                continue;
            switch(t.gameObject.layer)
            {
                case (int)Layers.enemy:
                    Debug.Log("HeadShot");
                    player.bulletCount--;
                    t.gameObject.SetActive(false);
                    break;
                case (int)Layers.boss:
                    Debug.Log("HeadShot");
                    player.bulletCount--;
                    t.gameObject.SetActive(false);
                    break;
            }
        }
        foreach (var c in player.UI.targetCrossHair)
        {
            c.gameObject.SetActive(false);
        }

        casting = false;
    }

    IEnumerator ScanTargets()
    {
        float timer = 0;
        Collider2D[] targets;
        while(true)
        {
            targets = Scanning();


            if (openFire)
            {
                Debug.Log("It's Highnoon");
                openFire = false;
                targets = Scanning();
                OpenFire(targets);
                yield break;
            }

            timer += Time.deltaTime;
            if(timer >= autoTargeting.scanTime)
            {
                casting = false;
                foreach (var c in player.UI.targetCrossHair)
                {
                    c.gameObject.SetActive(false);
                }
                yield break;
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // ½ºÄµ ¹üÀ§ »ö
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);

        // ½ºÄµ ¹üÀ§ ¿ø
        Gizmos.DrawWireSphere(player.transform.position, autoTargeting.scanRadius);
    }

    #endregion

    #region Flash Attack

    void FlashAttackCoolTime()
    {
        if (flashAttackUsable)
            return;

        flashAttackTimer -= Time.deltaTime;
        if(flashAttackTimer <= 0)
        {
            flashAttackUsable = true;
            flashAttackTimer = flashAttack.coolTime;
        }
    }

    public void InitiatingFlashAttack(bool facingRight)
    {
        if(!flashAttackUsable)
        {
            Debug.Log("Äð´Ù¿î");
            return;
        }

        casting = true;
        flashAttackUsable = false;
    }

    void PlayerMove(bool facingRight)
    {
        RaycastHit2D hit;

        if(facingRight)
        {
            hit = Physics2D.Raycast(player.transform.position, Vector2.right, flashAttack.attackDistance, flashAttack.scanable);
        }
        else
        {
            hit = Physics2D.Raycast(player.transform.position, Vector2.left, flashAttack.attackDistance, flashAttack.scanable);
        }
    }

    IEnumerator FlashAttack(bool facingRight)
    {


        yield return null;
    }

    #endregion 
}

