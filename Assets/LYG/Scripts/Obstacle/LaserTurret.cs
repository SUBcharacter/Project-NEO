using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] GameObject laser;
    [SerializeField] Transform target;
    [SerializeField] Transform muzzle;
    [SerializeField] LineRenderer aimLine;
    [SerializeField] Detector detector;
    [SerializeField] Quaternion origin;

    [SerializeField] float laserLength;
    [SerializeField] float aimTrackingSpeed;
    [SerializeField] float relaxSpeed;

    [SerializeField] int health;
    [SerializeField] int maxHealth;

    private void Awake()
    {
        origin = transform.rotation;
        aimLine = GetComponent<LineRenderer>();
        detector = GetComponent<Detector>();
    }

    private void Update()
    {
        Aiming();
    }

    void Aiming()
    {
        target = detector.Detect();

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

    void Fire()
    {

    }
}
