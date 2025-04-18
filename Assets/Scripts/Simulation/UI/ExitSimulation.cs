using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitSimulation : MonoBehaviour
{
    public string sceneToLoad;

    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}