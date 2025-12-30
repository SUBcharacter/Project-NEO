using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoad : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SceneLoader(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
