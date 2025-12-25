using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Text nameLabel;
    [SerializeField] Text line;
    [SerializeField] Image dialoguePoint;

    public bool isFinished { get; private set; }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        dialoguePoint.enabled = false;
        canvas.enabled = false;
        isFinished = true;
    }

    public void Init(string actorName, string dialogue)
    {
        isFinished = false;
        nameLabel.text = actorName;
        line.text = "";
        canvas.enabled = true;
        StartCoroutine(TypeText(dialogue));
    }

    IEnumerator TypeText(string dialogue)
    {
        dialogue = dialogue.Replace("\\n", "\n");
        int count = 0;

        while (count <= dialogue.Length)
        {
            line.text = dialogue.Substring(0, count);
            count++;

            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Return))
            {
                break;
            }

            yield return CoroutineCasher.Wait(0.01f);
        }

        line.text = dialogue;

        dialoguePoint.enabled = true;

        while(true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Return))
            {
                canvas.enabled = false;
                isFinished = true;
                break;
            }
            
            yield return null;
        }
    }
}
