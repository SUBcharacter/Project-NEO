using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EventStep", menuName = "EventStep/Player/PlayerMoveable")]
public class PlayerMoveable : EventStep
{
    [SerializeField] bool moveable;

    public override IEnumerator Execute(Player player)
    {
        player.Moveable = moveable;
        yield return null;
    }
}
