using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainCore : MonoBehaviour
{
    [SerializeField] SpriteRenderer ren;
    [SerializeField] Light2D lit;
    [SerializeField] SubCore[] subCores;

    [SerializeField] int count;
    [SerializeField] int overloadAmount;

    [SerializeField] bool overload;

    private void Awake()
    {
        ren = GetComponent<SpriteRenderer>();
        lit = GetComponent<Light2D>();
        subCores = FindObjectsByType<SubCore>(FindObjectsSortMode.None);
        count = subCores.Length;
    }

    public void DestorySubCore()
    {
        count--;

        if(count <= overloadAmount)
        {
            if(!overload)
            {
                Debug.Log("Àû °­È­");
                overload = true;
                StartCoroutine(OverLoad());
                EventManager.Publish(Event.Enemy_Enhance);
            }
        }
    }

    IEnumerator OverLoad()
    {
        float t = 0;
        Color origin = ren.color;
        Color target = Color.red;

        while(t < 1.5f)
        {
            t += Time.deltaTime;
            float progress = t / 1.5f;

            ren.color = Color.Lerp(origin, target, progress);
            lit.color = Color.Lerp(origin, target, progress);

            yield return null;
        }

        ren.color = target;
        lit.color = target;
    }
}
