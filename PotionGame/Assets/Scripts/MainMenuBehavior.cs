using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuBehavior : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("World Condensed");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
