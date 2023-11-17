using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Starting : MonoBehaviour
{
    [SerializeField] GameObject MapChoose;
    private string mapDoLoad;

    public void StartGame()
    {

        mapDoLoad = MapChoose.GetComponent<UnityEngine.UI.Image>().sprite.ToString().Split(' ')[0];
        Debug.Log(mapDoLoad);
        SceneManager.LoadScene(mapDoLoad);
    }
}
