using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject objectToMoveAround;
    [SerializeField] float mouseSensitivity;
 
    void FixedUpdate ()
    {
        float rotateHorizontal = Input.GetAxis("Mouse X");
        float rotateVertical = Input.GetAxis("Mouse Y");

        if (Input.GetMouseButton(0))
            transform.RotateAround (objectToMoveAround.transform.position, -Vector3.up, rotateHorizontal * mouseSensitivity);
    }
}
