using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicCamera : MonoBehaviour
{
    float lookSpeed = 0.2f;
    float moveSpeed = 0.1f;
    float flySpeed = 0.05f;
    float rotationX = 0.0f;
    float rotationY = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            
            rotationX += Mouse.current.delta.ReadValue().x * lookSpeed; //  Input.GetAxis("Mouse X")
            rotationY += Mouse.current.delta.ReadValue().y * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

            float verticalAxis = 0;
            if (Keyboard.current.wKey.isPressed)
            {
                verticalAxis = 1;
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                verticalAxis = -1;
            }
            transform.position += transform.forward * moveSpeed * verticalAxis; //Input.GetAxis("Vertical");
            float horizontalAxis = 0;
            if (Keyboard.current.dKey.isPressed)
            {
                horizontalAxis = 1;
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                horizontalAxis = -1;
            }
            transform.position += transform.right * moveSpeed * horizontalAxis;

            float flyAxis = 0;
            if (Keyboard.current.eKey.isPressed)
            {
                flyAxis = 1;
            }
            if (Keyboard.current.qKey.isPressed)
            {
                flyAxis = -1;
            }
            transform.position += transform.up * flySpeed * flyAxis;
        }
    }
}