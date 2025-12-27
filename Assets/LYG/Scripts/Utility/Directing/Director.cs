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
    [SerializeField] protected Animator animator;
    [SerializeField] protected Player player;
    [SerializeField] protected List<Sequance> sequance;

    public Animator AniCon => animator;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        player = FindAnyObjectByType<Player>();
    }

    public abstract void Play(int sequanceIndex);

    public abstract IEnumerator PlaySequance(int sequanceIndex);
}
