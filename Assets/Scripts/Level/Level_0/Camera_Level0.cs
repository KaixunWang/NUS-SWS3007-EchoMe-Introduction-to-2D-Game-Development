using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Level0   : MonoBehaviour
{
    public Transform player; // Reference to the player GameObject
    public Vector3 origin; // Reference to the origin Transform (assign in Inspector)
    public Vector3 target; // Reference to the target Transform (assign in Inspector)
    public Vector3 offset = new Vector3(0, 0, 0);
    public float xBoundary1 = -0.5f;
    public float xBoundary2 = 0.5f;
    public float smoothSpeed = 10f;
    void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    void LateUpdate()
    {
        if (player.position.x > xBoundary1)
        {
            Vector3 desiredPosition = target+ offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
        else if( player.position.x < xBoundary2)
        {
            Vector3 desiredPosition = origin + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}
