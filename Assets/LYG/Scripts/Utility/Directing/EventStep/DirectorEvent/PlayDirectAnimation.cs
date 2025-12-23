using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/Director/PlayDirectAnimation")]
public class PlayDirectAnimation : EventStep
{
    [SerializeField] string scene;

    public override IEnumerator Execute(Player player)
    {
        Animator anicon = FindAnyObjectByType<Director>().AniCon;

        anicon.Play(scene);

        yield return null;
    }
}
