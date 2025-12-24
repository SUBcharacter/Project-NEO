using UnityEngine;

[CreateAssetMenu(fileName = "InfightingPattern", menuName = "TutoBoss/TutoBossPattern/Infighting")]
public class InfightingPattern : BossPattern
{
    private float stopDistance = 1.3f;
    private float moveSpeed = 4f;
    protected override Awaitable Execute()
    {
        throw new System.NotImplementedException();
    }
    public override void UpdatePattern()
    {
        
    }
    public override void OnAnimationEvent(string eventName)
    {
        
    }

    public override void ExitPattern()
    {
       
    }


    
}
