using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EventStep", menuName = "EventStep/Player/MovePlayer")]
public class MovePlayer : EventStep
{
    [SerializeField] float velocity;
    [SerializeField] float moveTime;
    [SerializeField] bool moveRight;

    public override IEnumerator Execute(Player player)
    {
        // player.AniCon.Play("Walk");
        if(moveRight)
        {
            player.Rigid.linearVelocityX = velocity;
        }
        else
        {
            player.Rigid.linearVelocityX = -velocity;
        }

        yield return CoroutineCasher.Wait(moveTime);

        // player.AniCon.Play("Idle");
        player.Rigid.linearVelocity = Vector2.zero;
    }
}
