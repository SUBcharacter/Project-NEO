using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Player/PlayerUIActive")]
public class PlayerUIActive : EventStep
{
    [SerializeField] bool value;

    public override IEnumerator Execute(Player player)
    {
        player.UI.Cnvs.enabled = value;
        yield return null;
    }
}
