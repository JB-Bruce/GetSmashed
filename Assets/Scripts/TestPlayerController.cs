using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : MonoBehaviour
{
    public float speed;
    Vector2 inputValues;

    private void Update()
    {
        transform.Translate(inputValues * speed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputValues = context.ReadValue<Vector2>();        
    }
}
