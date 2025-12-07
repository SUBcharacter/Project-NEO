using UnityEngine;

public class EnergyOrb : Orb
{
    protected override void Awake()
    {
        player = FindAnyObjectByType<Player>();
        rigid = GetComponent<Rigidbody2D>();
        timer = lifeTime;
        int rand = Random.Range(8,12);
        amount = rand;
    }

    protected override void Update()
    {
        timer -= Time.deltaTime;
        dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

        rigid.AddForce(dir * speed, ForceMode2D.Impulse);

        if (timer <= 0)
        {
            player.GetOverFlowEnergy(amount);
            gameObject.SetActive(false);
        }
    }

    public override void Init(Vector3 pos)
    {
        transform.position = pos;
        int rand = Random.Range(8, 12);
        amount = rand;
        timer = lifeTime;
        gameObject.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & absorbable) == 0)
            return;

        player.GetOverFlowEnergy(amount);
        gameObject.SetActive(false);
    }

}
