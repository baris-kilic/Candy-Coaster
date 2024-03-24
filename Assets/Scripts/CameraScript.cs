using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : Singleton<CameraScript>
{

    public Renderer rend;
    public Camera cam;

    private void Start()
    {
        
    }

    public void ArrangeCam(Vector2Int pos)
    {
        rend.gameObject.transform.localScale = new Vector3(pos.x, pos.y);

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = rend.bounds.size.x / rend.bounds.size.y;

        if (screenRatio >= targetRatio)
        {
            cam.orthographicSize = rend.bounds.size.y / 2;
        }
        else
        {
            float diffirenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = rend.bounds.size.y / 2 * diffirenceInSize;
        }
    }
}