using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpRotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camPosition = Camera.allCameras[0].transform.position;
        camPosition.y += .3f;

        transform.LookAt(camPosition);
        transform.RotateAround(transform.position, Vector3.up, 180);
    }
}
