using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // loads scene
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene + 1);  // its +1 because I forgot that the 0th scene is the first scene displayed when the game runs and this was quicker than changing 12 variables
    }

    // quits the game
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
