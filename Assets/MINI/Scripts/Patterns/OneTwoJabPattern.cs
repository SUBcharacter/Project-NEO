using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(fileName = "OneTwoJabPattern", menuName = "Boss/Patterns/OneTwoJab")]
public class OneTwoJabPattern : BossPattern
{

    protected override async Awaitable Execute()
    {
        if (boss == null) return;

        try
        {
            await Awaitable.WaitForSecondsAsync(1f, boss.DestroyCancellationToken);
        }
        catch (System.OperationCanceledException) { return; }
        
    }
    public override void UpdatePattern() {}
    public override void ExitPattern() { }
    public override void OnAnimationEvent(string eventName) { }
}
