using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string sceneToLoad;

    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}