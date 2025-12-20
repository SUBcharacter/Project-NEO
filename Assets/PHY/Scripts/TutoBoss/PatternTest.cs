using UnityEngine;

public class PatternTest : MonoBehaviour
{
    [SerializeField] private BossAI boss;
    [SerializeField] private BossPattern testPattern;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(boss == null || testPattern == null)
        {
            Debug.LogError("boss ¶Ç´Â testPatternÀÌ null");
            return;
        }

        testPattern.Initialize(boss);
        testPattern.StartPattern();
    }

 
}
