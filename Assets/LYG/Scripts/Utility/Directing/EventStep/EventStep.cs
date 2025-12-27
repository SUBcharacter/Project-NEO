using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EventStep", menuName = "Scriptable Objects/EventStep")]
public abstract class EventStep : ScriptableObject
{
    public abstract IEnumerator Execute(Player player);
}
