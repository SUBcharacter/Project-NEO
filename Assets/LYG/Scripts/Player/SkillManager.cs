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
    [SerializeField] GameObject slashHitBox;
    [SerializeField] Player player;
    [SerializeField] Magazine knifePool;

    [SerializeField] float phantomBladeTimer;
    [SerializeField] float chargeAttackTimer;
    [SerializeField] float autoTargetingTimer;
    [SerializeField] float flashAttackTimer;

    [SerializeField] bool casting;
    [SerializeField] bool charging;
    [SerializeField] bool openFire;
    [SerializeField] bool phantomBladeUsable;
    [SerializeField] bool chargeAttackUsable;
    [SerializeField] bool autoTargetingUsable;
    [SerializeField] bool flashAttackUsable;

    public SkillStat PhantomBladeStat => phantomBlade;
    public SkillStat ChargeAttackStat => chargeAttack;
    public SkillStat AutoTargetingStat => autoTargeting;
    public SkillStat FlashAttackStat => flashAttack;

    public float PhantomBladeTimer => phantomBladeTimer;
    public float ChargeAttackTimer => chargeAttackTimer;
    public float AutoTargetingTimer => autoTargetingTimer;
    public float FlashAttackTimer => flashAttackTimer;


    public bool Casting => casting;
    public bool Charging => charging;
    public bool _OpenFire => openFire;
    public bool PhantomBladeUsable => phantomBladeUsable;
    public bool ChargeAttackUsable => chargeAttackUsable;
    public bool AutoTargetingUsable => autoTargetingUsable;
    public bool FlashAttackUsable => flashAttackUsable;

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
        FlashAttackCoolTime();
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
            Debug.Log("쿨다운");
            return;
        }
        phantomBladeUsable = false;
        casting = true;
        StartCoroutine(PhantomBlade(spawnPoint, dir));
    }

    IEnumerator PhantomBlade(Transform[] spawnPoint, Vector2 dir)
    {
        if (player.BulletCount >= phantomBlade.bulletCost)
        {
            player.BulletCount -= phantomBlade.bulletCost;
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
            Debug.Log("스태미나 부족");
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
            Debug.Log("쿨다운");
            return;
        }
        chargeAttackUsable = false;
        casting = true;
        StartCoroutine(ChargeAttack(dir));
    }

    public IEnumerator ChargeAttack(Vector2 dir)
    {
        if (player.Stamina >= chargeAttack.staminaCost)
        {
            player.GhTr.gameObject.SetActive(true);
            charging = true;
            player.Stamina -= chargeAttack.staminaCost;
            Debug.Log("Chaaarge");
            LayerMask originMask = player.gameObject.layer;
            float gravityScale = player.Rigid.gravityScale;
            float velocity = 0;

            player.Rigid.gravityScale = 0;
            player.gameObject.layer = LayerMask.NameToLayer("Invincible");
            player.Rigid.linearVelocity = Vector2.zero;
            player.Col.isTrigger = true;
            yield return CoroutineCasher.Wait(0.01f);
            chargeHitBox.Init();
            while (true)
            {

                velocity = Mathf.MoveTowards(velocity, chargeAttack.chargeSpeed, chargeAttack.chargeAccel * Time.deltaTime);
                player.Rigid.linearVelocity = dir * velocity;

                if (chargeHitBox.Trigger)
                {
                    player.Rigid.linearVelocity = Vector2.zero;
                    chargeHitBox.gameObject.SetActive(false);

                    RaycastHit2D hit = Physics2D.Raycast(player.transform.position, dir, 5f, chargeHitBox.Stats.attackable);
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
                    CameraShake.instance.Shake(4, 0.2f);
                    player.Rigid.linearVelocity = knockDir * chargeAttack.knockBackForce;
                    Debug.Log("부딪혀잇");
                    break;
                }

                if (Mathf.Abs(velocity) >= chargeAttack.chargeSpeed - 1f)
                {
                    player.Rigid.linearVelocity = Vector2.zero;
                    chargeHitBox.gameObject.SetActive(false);
                    break;
                }

                yield return null;
            }

            player.Rigid.gravityScale = gravityScale;
            player.Col.isTrigger = false;
            player.GhTr.gameObject.SetActive(false);
            yield return CoroutineCasher.Wait(0.3f);

            player.gameObject.layer = originMask;
            charging = false;
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
            Debug.Log("쿨다운");
            return;
        }

        if(!casting)
        {
            Debug.Log("스캔중");
            casting = true;
            StartCoroutine(ScanTargets());
        }
        else
        {
            Debug.Log("일제 사격");
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
            Debug.Log("타겟 없음");
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
            openFire = false;
            casting = false;
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
                    player.BulletCount--;
                    t.gameObject.SetActive(false);
                    break;
                case (int)Layers.boss:
                    Debug.Log("HeadShot");
                    player.BulletCount--;
                    t.gameObject.SetActive(false);
                    break;
            }
        }
        foreach (var c in player.UI.targetCrossHair)
        {
            c.gameObject.SetActive(false);
        }
        openFire = false;
        casting = false;
    }

    IEnumerator ScanTargets()
    {
        if(player.BulletCount >= autoTargeting.bulletCost)
        {
            float timer = 0;
            Collider2D[] targets;
            while (true)
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
                if (timer >= autoTargeting.scanTime)
                {
                    casting = false;
                    foreach (var c in player.UI.targetCrossHair)
                    {
                        c.gameObject.SetActive(false);
                    }
                    autoTargetingUsable = false;
                    autoTargetingTimer = 0.5f;
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            Debug.Log("최소 코스트 부족");
            casting = false;
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // 스캔 범위 색
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);

        // 스캔 범위 원
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
            Debug.Log("쿨다운");
            return;
        }

        casting = true;
        flashAttackUsable = false;
        StartCoroutine(FlashAttack(facingRight));
    }

    float CalculateDistance(bool facingRight, Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, dir, flashAttack.attackDistance, flashAttack.scanable);

        float tuning = player.Col.size.x / 2;
        float distance;
        if (hit.collider != null)
        {
            distance = hit.distance - tuning;
        }
        else
        {
            distance = flashAttack.attackDistance;
        }

        Debug.Log(facingRight ? "오른쪽으로 텔포!" : "왼쪽으로 텔포!");
        return distance;
    }

    GameObject CreateHitBox(float distance, Vector2 dir)
    {
        Vector3 origin = player.transform.position;
        Vector3 position = new Vector3(origin.x + ((distance/2) * dir.x), origin.y,origin.z);
        float tuning = 8 / flashAttack.attackDistance;

        GameObject hitbox = Instantiate(slashHitBox, position, Quaternion.identity);

        hitbox.transform.localScale = new Vector3(distance * tuning, 1, 1);

        return hitbox;
    }

    IEnumerator FlashAttack(bool facingRight)
    {
        if (player.Stamina >= flashAttack.staminaCost)
        {
            player.Stamina -= flashAttack.staminaCost;
            CameraShake.instance.Shake(4, 0.2f);
            player.Stamina -= flashAttack.staminaCost;
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            float distance = CalculateDistance(facingRight, dir);

            GameObject hitBox = CreateHitBox(distance, dir);

            player.transform.position += new Vector3(distance * dir.x, 0, 0);
            casting = false;
            yield return CoroutineCasher.Wait(0.1f);

            Destroy(hitBox);
        }
        else
        {
            Debug.Log("스태미나 부족");
            casting = false;
        }
    }

    #endregion 
}

