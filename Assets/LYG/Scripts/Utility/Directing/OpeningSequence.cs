using UnityEngine;

public class OpeningSequence : MonoBehaviour
{
    public void FadeIn()
    {
        FadeControler.instance.TryFadeIn(3f);
    }
    
    public void FadeOut()
    {
        FadeControler.instance.TryFadeOut(3f);
    }
}
