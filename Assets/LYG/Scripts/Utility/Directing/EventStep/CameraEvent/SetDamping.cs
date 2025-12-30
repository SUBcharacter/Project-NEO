using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Camera/SetDamping")]
public class SetDamping : EventStep
{
    [SerializeField] Vector2 damping;

    public override IEnumerator Execute(Player player)
    {
        CameraManager.instance.SetDamping(damping);

        yield return null;
    }
}
