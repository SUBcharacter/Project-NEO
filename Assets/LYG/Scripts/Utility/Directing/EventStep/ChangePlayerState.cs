using System.Collections;
using UnityEngine;

public class ChangePlayerState : EventStep
{
    [SerializeField] string stateName;

    public override IEnumerator Execute(Player player)
    {
        player.ChangeState(player.States[stateName]);
        yield return null;
    }
}
