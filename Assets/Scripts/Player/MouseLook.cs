using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSens = 100f;

    public Transform playerBody;

    public static bool frozen;

    float xRotation = 0f;
    GameObject[] camFollows;
    // Start is called before the first frame update
    void Start()
    {
        frozen = false;
        Cursor.lockState = CursorLockMode.Locked;
        camFollows = GameObject.FindGameObjectsWithTag("camFollow");
    }

    // Update is called once per frame
    void Update()
    {
        //Basic mouse-based camera control system.
        if (!frozen)
        {
            //Simple equation: axis times sensitivity times time between frames.
            float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

            //Clamp y rotation by 90 degrees in either direction to prevent "weird" looking mechanic.
            //Mouse x and Mouse y flipped from intuition because of weird unity shenanigans.
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //Full body rotates on x rot, only camera on y rot.
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
