using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Camera/SetDeadzone")]
public class SetDeadzone : EventStep
{
    [SerializeField] Vector2 size;

    public override IEnumerator Execute(Player player)
    {
        CameraManager.instance.DeadZoneControl(size);

        yield return null;
    }
}
