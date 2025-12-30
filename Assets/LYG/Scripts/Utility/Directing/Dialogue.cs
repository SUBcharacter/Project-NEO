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

    bool skipRequested = false;
    public bool isFinished { get; private set; }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        dialoguePoint.enabled = false;
        canvas.enabled = false;
        isFinished = true;
    }

    public void Init(string actorName, string dialogue, float speed)
    {
        isFinished = false;
        skipRequested = false;
        nameLabel.text = actorName;
        line.text = "";
        canvas.enabled = true;
        StartCoroutine(TypeText(dialogue, speed));
    }

    bool DialogueInput()
    {
        return (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Return));
    }

    string RemoveTag(string dialogue)
    {
        while(true)
        {
            int start = dialogue.IndexOf('<');
            if (start == -1) break;
            int end = dialogue.IndexOf('>',start);
            if (end == -1) break;

            dialogue = dialogue.Remove(start, end - start + 1);
        }
        return dialogue;
    }

    IEnumerator WaitWithSkip(float time)
    {
        float timer = 0;

        while(timer < time)
        {
            if(DialogueInput())
            {
                skipRequested = true;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator TypeText(string dialogue, float speed)
    {
        dialogue = dialogue.Replace("\\n", "\n");
        int index = 0;
        float timer = 0;

        while (index < dialogue.Length)
        {
            yield return null;

            if (DialogueInput())
            {
                skipRequested = true;
            }

            if (dialogue[index] == '<')
            {
                int end = dialogue.IndexOf('>', index);

                if(end == -1)
                {
                    Debug.LogError("Wait 태그 에러");
                    break;
                }

                string tag = dialogue.Substring(index + 1, end - index - 1);

                if(tag.StartsWith("wait="))
                {
                    float waitTime = float.Parse(tag.Replace("wait=", ""));
                    yield return WaitWithSkip(waitTime);
                }
                index = end + 1;

                if (skipRequested)
                {
                    break;
                }

                continue;
            }

            if (skipRequested)
            {
                break;
            }

            timer += Time.deltaTime;

            if(timer >= speed)
            {
                timer = 0;
                line.text += dialogue[index++];
            }
            
        }

        line.text = RemoveTag(dialogue);

        dialoguePoint.enabled = true;

        while(true)
        {
            yield return null;

            if (DialogueInput())
            {
                canvas.enabled = false;
                isFinished = true;
                skipRequested = false;
                break;
            }
            
        }
    }
}
