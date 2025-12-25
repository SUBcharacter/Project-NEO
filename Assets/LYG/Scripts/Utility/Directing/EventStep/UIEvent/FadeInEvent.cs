using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/UIEvent/FadeInEvent")]
public class FadeInEvent : EventStep
{
    [SerializeField] FadeControler prefab;

    [SerializeField] float duration;

    public override IEnumerator Execute(Player player)
    {
        FadeControler fadeControler = FindAnyObjectByType<FadeControler>();

        if (fadeControler != null)
        {
            fadeControler.TryFadeIn(duration);
        }
        else
        {
            fadeControler = Instantiate(prefab);
            fadeControler.TryFadeIn(duration);
        }
        yield return null;
    }
}
