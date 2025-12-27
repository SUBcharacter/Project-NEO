using System.Collections;
using UnityEngine;

public class Scene2Direct : Director
{
    protected override void Start()
    {
        base.Start();
        Play(0);
    }

    public override void Play(int sequanceIndex)
    {
        StartCoroutine(PlaySequance(sequanceIndex));
    }

    public override IEnumerator PlaySequance(int sequanceIndex)
    {
        foreach (var step in sequance[sequanceIndex].eventSteps)
        {
            yield return step.Execute(player);
        }
    }
}
