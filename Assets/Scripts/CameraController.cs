using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 offset;


    public float cameraFOV = 100.0f;

    private float screenWidth;
    private float screenHeight;

    void Start()
    {
        offset = transform.position;

        // Ekran boyutunu ve çözünürlüðünü al
        screenWidth = Screen.width;
        screenHeight = Screen.height;

    
        float aspectRatio = screenWidth / screenHeight;
        Camera.main.fieldOfView = cameraFOV / aspectRatio;
    }

    void Update()
    {

        if (screenWidth != Screen.width || screenHeight != Screen.height)
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            float aspectRatio = screenWidth / screenHeight;
            Camera.main.fieldOfView = cameraFOV / aspectRatio;
        }
    }
}
