using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void Map()
    {
        SceneManager.LoadScene("Map");
    }
    public void MenuSelection()
    {
        SceneManager.LoadScene("MenuSelection");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LeaveGame()
    {
        Application.Quit();
    }
}
