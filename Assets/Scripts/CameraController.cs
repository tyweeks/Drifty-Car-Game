using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private CameraMode cameraMode = CameraMode.Overview;

    private enum CameraMode
    {
        Overview = 0,
        Player = 1
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cameraMode == CameraMode.Overview)
            {
                cameraMode = CameraMode.Player;
            }
            else
            {
                cameraMode = CameraMode.Overview;
                transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                transform.position = new Vector3(0.0f, 90.0f, 0.0f);
            }
        }

        if (cameraMode == CameraMode.Player)
        {
            //transform.position = player.transform.position + offset;
            transform.eulerAngles = new Vector3(30.0f, player.transform.eulerAngles.y + 90.0f, 0.0f);
            transform.position = player.transform.TransformPoint(new Vector3(-5.0f, 5.0f, 0.0f));
        }
    }
}
