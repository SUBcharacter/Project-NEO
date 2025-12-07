using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] GameObject laser;
    [SerializeField] Transform target;
    [SerializeField] Transform muzzle;
    [SerializeField] LineRenderer aimLine;
    [SerializeField] Detector detector;
    [SerializeField] Coroutine fire;
    [SerializeField] Quaternion origin;
    [SerializeField] LayerMask hitMask;

    [SerializeField] float laserLength;
    [SerializeField] float aimTrackingSpeed;
    [SerializeField] float relaxSpeed;
    [SerializeField] float timer;
    [SerializeField] float aimTime;

    [SerializeField] int health;
    [SerializeField] int maxHealth;

    [SerializeField] bool stop;

    private void Awake()
    {
        stop = false;
        origin = transform.rotation;
        aimLine = GetComponent<LineRenderer>();
        detector = GetComponent<Detector>();
    }

    private void OnEnable()
    {
        EventManager.Subscribe(Event.Stop, Stop);
        EventManager.Subscribe(Event.Play, Play);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(Event.Stop, Stop);
        EventManager.Unsubscribe(Event.Play, Play);
    }

    private void Update()
    {
        if (stop)
            return;
        Activate();
    }

    void Stop()
    {
        Debug.Log("≈Õ∑ø ¡§¡ˆ");
        stop = true;
    }

    void Play()
    {
        Debug.Log("≈Õ∑ø ∞°µø");
        stop = false;
    }

    void Aiming()
    {
        if(target != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)target.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

            Quaternion targetRot = Quaternion.Euler(0, 0, angle);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * aimTrackingSpeed);

            aimLine.enabled = true;
            aimLine.SetPosition(0, muzzle.position);
            Vector3 endPos = muzzle.position + transform.up * laserLength;
            aimLine.SetPosition(1, endPos);
        }
        else
        {
            aimLine.enabled = false;
            transform.rotation = Quaternion.Lerp(transform.rotation, origin, Time.deltaTime * relaxSpeed);
        }
    }

    void Activate()
    {
        target = detector.Detect();

        if(target != null)
        {
            if(timer < aimTime)
            {
                Aiming();
                timer += Time.deltaTime;
            }
            else
            {
                if (fire == null)
                    fire = StartCoroutine(Fire());
            }
        }
        else
        {
            if(fire == null)
            {
                Aiming();
                timer = 0;
            }
        }
    }

    IEnumerator Fire()
    {
        aimLine.enabled = false;
        yield return CoroutineCasher.Wait(0.5f);

        RaycastHit2D hit = Physics2D.Raycast(muzzle.position, transform.up, laserLength, hitMask);
        float waitTimer = 0;
        GameObject hitBox;
        if(hit.collider != null)
        {
            float distance = hit.distance;
            Vector2 plusPosition = transform.up * (distance/2);
            Vector2 position = new Vector2((muzzle.position.x + plusPosition.x), (muzzle.position.y + plusPosition.y));
            hitBox = Instantiate(laser,position,transform.rotation);
            hitBox.transform.localScale = new Vector3(1, distance + 1, 1);
        }
        else
        {
            Vector2 plusPosition = transform.up * (laserLength / 2);
            Vector2 position = new Vector2((muzzle.position.x + plusPosition.x),(muzzle.position.y + plusPosition.y));
            hitBox = Instantiate(laser, position, transform.rotation);
            hitBox.transform.localScale = new Vector3(1, laserLength + 1, 1);
        }

        while(true)
        {
            if (stop)
            {
                yield return null;
                continue;
            }

            waitTimer += Time.deltaTime;
            if (waitTimer >= 2f)
                break;

            yield return null;
        }

        timer = 0;
        Destroy(hitBox);
        fire = null;
    }
}
