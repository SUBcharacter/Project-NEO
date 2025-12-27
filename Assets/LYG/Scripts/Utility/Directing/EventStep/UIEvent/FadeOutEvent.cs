using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/UIEvent/FadeOutEvent")]
public class FadeOutEvent : EventStep
{
    [SerializeField] FadeControler prefab;

    [SerializeField] float duration;

    public override IEnumerator Execute(Player player)
    {
        FadeControler fadeControler = FindAnyObjectByType<FadeControler>();

        if (fadeControler != null)
        {
            fadeControler.TryFadeOut(duration);
        }
        else
        {
            fadeControler = Instantiate(prefab);
            fadeControler.TryFadeOut(duration);
        }
        yield return null;
    }
}
