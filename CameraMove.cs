using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour
{
    public static Vector3 rotationCenter = new Vector3(0, 0, 0);
    public static float sensitivityMouse = 10f;
    public static float sensitivetyKeyBoard = 0.1f;
    public static float sensitivetyMouseWheel = 10f;
    public static float cameraRadius = 0.0f;


    float x = 0;
    float y = 0;

    void Start()
    {
        rotationCenter = new Vector3(0,0,0);
        cameraRadius = transform.position.magnitude;
        Debug.Log(transform.position);
    }
    public int setRotationCenter(Vector3 center_)
    {
        rotationCenter = new Vector3(center_.x, center_.y, center_.z);
        cameraRadius = (transform.position- rotationCenter).magnitude;
        return 0;
    }
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * 15 * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * 15 * Time.deltaTime;
            Quaternion q = Quaternion.Euler(y, x, 0);
            Vector3 direction = q * Vector3.forward;
            this.transform.position = rotationCenter - direction * cameraRadius;
            this.transform.LookAt(rotationCenter);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (Camera.main.fieldOfView <= 100)
                Camera.main.fieldOfView += 2;
            if (Camera.main.orthographicSize <= 20)
                Camera.main.orthographicSize += 0.5F;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (Camera.main.fieldOfView > 2)
                Camera.main.fieldOfView -= 2;
            if (Camera.main.orthographicSize >= 1)
                Camera.main.orthographicSize -= 0.5F;
        }
    }
}


    