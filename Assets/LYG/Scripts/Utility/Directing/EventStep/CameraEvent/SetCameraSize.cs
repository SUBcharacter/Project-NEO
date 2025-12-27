using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Camera/SetCameraSize")]
public class SetCameraSize : EventStep
{
    [SerializeField] float amount;
    [SerializeField] float duration;

    public override IEnumerator Execute(Player player)
    {
        CameraManager.instance.ZoomControl(amount, duration);
        yield return null;
    }
}
