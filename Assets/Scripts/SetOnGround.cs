using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOnGround : MonoBehaviour
{
    private bool justSpawned;

    // Start is called before the first frame update
    void Start()
    {
        justSpawned = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (justSpawned)
        {
            Rigidbody body;
            if ((body = GetComponent<Rigidbody>()) != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1000.0f))
                {
                    transform.position = hit.point;
                    justSpawned = false;
                }
                else 
                {
                    Debug.Log("Could not find ground!");
                }
            }
        }
    }
}
