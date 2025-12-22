using System.Collections;
using UnityEngine;

public class PlayerMoveable : EventStep
{
    [SerializeField] bool moveable;

    public override IEnumerator Execute(Player player)
    {
        player.Moveable = moveable;
        yield return null;
    }
}
