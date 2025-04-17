using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WelcomeScreen : MonoBehaviour
{
    public void StartSimulation()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        //Debug.Log("Quit button pressed!");

        #if UNITY_EDITOR
            EditorApplication.isPlaying = false; //when simulating in Unity Editor, will stop playing/simulates quiting application
        #else
            Application.Quit(); //will quit in build
        #endif
    }
}
