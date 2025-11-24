using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private void Update()
    {
        float hz = Input.GetAxis("Horizontal");
        float vt = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(hz, 0, 0) * speed * Time.deltaTime;
        transform.position += move;
    }


}
