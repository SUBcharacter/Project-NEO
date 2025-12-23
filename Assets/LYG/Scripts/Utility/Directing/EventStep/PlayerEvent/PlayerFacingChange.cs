using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EventStep", menuName = "EventStep/Player/PlayerFacingChange")]
public class PlayerFacingChange : EventStep
{
    [SerializeField] bool facingRight;

    public override IEnumerator Execute(Player player)
    {
        player.FlipX(facingRight);
        yield return null;
    }
}
