using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string sceneToLoad;

    public void SwitchScene()
    {
        Cursor.visible = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}