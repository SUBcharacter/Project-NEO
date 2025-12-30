using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUIActive", menuName = "EventStep/UIEvent/DialogueStep")]
public class DialogueStep : EventStep
{
    [SerializeField] Dialogue prefab;
    
    [SerializeField] string actorName;
    [SerializeField] string line;

    [SerializeField] float speed = 0.03f;

    public override IEnumerator Execute(Player player)
    {
        Debug.Log(line);
        Dialogue dialogue = FindAnyObjectByType<Dialogue>();
        if(dialogue != null)
        {
            dialogue.Init(actorName, line, speed);
        }
        else
        {
            dialogue = Instantiate(prefab);
            dialogue.Init(actorName, line, speed);
        }

        yield return new WaitUntil(() => dialogue.isFinished);
    }
}
