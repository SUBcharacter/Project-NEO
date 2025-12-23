using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Waiting")]
public class Waiting : EventStep
{
    [SerializeField] float time;

    public override IEnumerator Execute(Player player)
    {
        yield return CoroutineCasher.Wait(time);
    }
}
