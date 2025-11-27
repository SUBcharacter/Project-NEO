using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] SkillStat phantomBlade;
    [SerializeField] SkillStat chargeAttack;
    [SerializeField] SkillStat autoTargeting;
    [SerializeField] HitBox chargeHitBox;
    [SerializeField] Player player;
    public Magazine knifePool;

    public float phantomBladeTimer;
    public float chargeAttackTimer;
    public float autoTargetingTimer;

    public bool casting;
    public bool openFire;
    public bool phantomBladeUsable;
    public bool chargeAttackUsable;
    public bool autoTargetingUsable;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        knifePool = GetComponent<Magazine>();
        casting = false;
        phantomBladeUsable = true;
        chargeAttackUsable = true;
    }

    private void Update()
    {
        Debug.Log(chargeHitBox.triggered);
        PhantomBladeCoolTime();
        ChargeAttackCoolTime();
    }

    #region Phantom Blade

    void PhantomBladeCoolTime()
    {
        if (phantomBladeUsable)
            return;
        phantomBladeTimer += Time.deltaTime;

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
                spawnPoints[i] = spawnPoint[i].position;
            }

            for (int i = 0; i < phantomBlade.attackCount; i++)
            {
                while (true)
                {
                    index = Random.Range(0, spawnPoint.Length);

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

    public void InitiatingAutoTargeting()
    {
        if(!casting)
        {
            casting = true;
            StartCoroutine(ScanTargets());
        }
        else
        {
            
        }
    }

    IEnumerator ScanTargets()
    {
        float timer = 0;
        while(true)
        {

            if (openFire)
            {
                yield break;
            }

            timer += Time.deltaTime;
            if(timer >= autoTargeting.scanTime)
            {
                casting = false;

                yield break;
            }

            yield return null;
        }
    }

    #endregion
}

