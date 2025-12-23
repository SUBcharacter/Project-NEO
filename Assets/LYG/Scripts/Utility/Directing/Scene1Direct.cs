using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene1Direct : Director
{
    public static Scene1Direct instance;

    protected override void Start()
    {
        base.Start();
        instance = this;
        Play(0);
    }

    public override void Play(int sequanceIndex)
    {
        StartCoroutine(PlaySequance(sequanceIndex));
    }

    public override IEnumerator PlaySequance(int sequanceIndex)
    {
        foreach(var step in sequance[sequanceIndex].eventSteps)
        {
            yield return step.Execute(player);
        }
    }

}
