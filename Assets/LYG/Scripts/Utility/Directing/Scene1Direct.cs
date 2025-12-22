using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene1Direct : Director
{

    protected override void Start()
    {
        base.Start();
    }

    public override void Play(int sceneIndex)
    {
        StartCoroutine(PlaySequance(sceneIndex));
    }

    public override IEnumerator PlaySequance(int sceneIndex)
    {
        foreach(var step in sequance[sceneIndex].eventSteps)
        {
            yield return step.Execute(player);
        }
    }

}
