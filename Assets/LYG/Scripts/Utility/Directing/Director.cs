using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sequance
{
    public List<EventStep> eventSteps;
}

public abstract class Director : MonoBehaviour
{
    [SerializeField] protected Player player;
    [SerializeField] protected List<Sequance> sequance;
    
    

    protected virtual void Start()
    {
        player = FindAnyObjectByType<Player>();
    }

    public abstract void Play(int sceneIndex);

    public abstract IEnumerator PlaySequance(int sceneIndex);
}
