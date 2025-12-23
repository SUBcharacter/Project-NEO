using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Camera/CameraTargetPlayer")]
public class CameraTargetPlayer : EventStep
{
    public override IEnumerator Execute(Player player)
    {
        CameraManager.instance.SetTrackingTarget(player.transform);
        yield return null;
    }
}
