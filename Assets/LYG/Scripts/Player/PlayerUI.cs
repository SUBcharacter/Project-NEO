using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Image targetCrossHairPrefab;
    public Image playerCrossHair;
    public List<Image> targetCrossHair;

    private void Awake()
    {
        for(int i = 0; i< 10; i++)
        {
            Image instance = Instantiate(targetCrossHairPrefab, transform);
            targetCrossHair.Add(instance);
            targetCrossHair[i].gameObject.SetActive(false);
        }
    }
}
