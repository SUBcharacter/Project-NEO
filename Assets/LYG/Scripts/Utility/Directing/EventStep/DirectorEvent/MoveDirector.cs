using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Director/MoveDirector")]
public class MoveDirector : EventStep
{
    [SerializeField] Vector3 position;

    public override IEnumerator Execute(Player player)
    {
        Director director = FindAnyObjectByType<Director>();

        director.transform.position = position;

        yield return null;
    }
}
