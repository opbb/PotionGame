using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    Transform playerBody;
    public float mouseSensitivity = 150f;
    public static bool isUIActive; 

    float pitch = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = transform.parent.transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isUIActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isUIActive)
        {
            float moveX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Apply yaw rotation to player body
            playerBody.Rotate(Vector3.up * moveX);

            // Apply pitch to camera
            pitch -= moveY;

            // limit player from rotating all the way upside down
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
        
    }
}
