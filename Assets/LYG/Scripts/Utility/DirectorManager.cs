using UnityEngine;

public class DirectorManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] GameObject[] point;

    [SerializeField] int index;


    private void Awake()
    {
        player = FindAnyObjectByType<Player>();

        index = 0;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            CameraManager.instance.SetTrackingTarget(point[index].transform);
            index = (index + 1) % point.Length;
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            CameraManager.instance.SetTrackingTarget(player.transform);
        }

        if(Input.GetKeyDown(KeyCode.Minus))
        {
            CameraManager.instance.ZoomControl(-2, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            CameraManager.instance.ZoomControl(2, 1f);
        }
    }


}
