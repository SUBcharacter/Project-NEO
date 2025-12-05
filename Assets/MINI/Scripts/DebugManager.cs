using UnityEngine;
using UnityEngine.Rendering;

public class DebugManager : MonoBehaviour
{
    [SerializeField] BossAI controller;
    [SerializeField] PlayerMove player;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject bossHitBox;

    [SerializeField] GameObject bulletObject;
    private bool isLeft = true;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            bulletObject = Instantiate(bulletPrefab, player.transform.position, Quaternion.identity);
        }
        IsPlayerVector();
        bulletMove();
    }

    void bulletMove()
    {
        if (bulletObject != null)
        {
            if (isLeft)
            {
                bulletObject.transform.Translate(Vector3.left * 10f * Time.deltaTime);
            }
            else
            {
                bulletObject.transform.Translate(Vector3.right * 10f * Time.deltaTime);
            }
        }
    }
    void IsPlayerVector()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            isLeft = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            isLeft = false;
        }
    }
}
