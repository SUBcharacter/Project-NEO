using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Camera/CameraTargetDirector")]
public class CameraTargetDirector : EventStep
{


    public override IEnumerator Execute(Player player)
    {
        Transform target = FindAnyObjectByType<Director>().transform;
        CameraManager.instance.SetTrackingTarget(target);

        yield return null;
    }
}
