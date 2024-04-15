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
        Vector3 dirVec = transform.position - camPosition;
        Vector3 negCamPosition = transform.position + dirVec;

        transform.LookAt(negCamPosition);
    }
}
