using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Parallaxe : MonoBehaviour
{
    Vector3 Repop;

    float LeftBorder;
    float RightBorder;
    float Speed;
    [SerializeField] float MinSpeed = 1f;
    [SerializeField] float MaxSpeed = .5f;

    // Start is called before the first frame update
    void Start()
    {
        LeftBorder = transform.parent.position.x - 30;
        RightBorder = transform.parent.position.x + 30;
        Repop = new Vector3(RightBorder, transform.position.y, transform.position.z);
        Speed = Random.Range(MinSpeed, MaxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x - Speed * Time.deltaTime, transform.position.y, transform.position.z);
        if (transform.position.x <= LeftBorder)
        {
            transform.position = Repop;
        }
    }
}
