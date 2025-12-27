using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/UIEvent/DialogueStep")]
public class DialogueStep : EventStep
{
    [SerializeField] Dialogue prefab;
    
    [SerializeField] string actorName;
    [SerializeField] string line;

    public override IEnumerator Execute(Player player)
    {
        Dialogue dialogue = FindAnyObjectByType<Dialogue>();
        if(dialogue != null)
        {
            dialogue.Init(actorName, line);
        }
        else
        {
            dialogue = Instantiate(prefab);
            dialogue.Init(actorName, line);
        }

        yield return new WaitUntil(() => dialogue.isFinished);
    }
}
