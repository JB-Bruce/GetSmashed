using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MapChoose : MonoBehaviour
{
    [SerializeField] GameObject displayer;
    private UnityEngine.UI.Image image;

    private void Start()
    {
        image = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
    }

    public void Display()
    {
        displayer.GetComponent<UnityEngine.UI.Image>().sprite = image.sprite;

        displayer.SetActive(true);
    }
}
