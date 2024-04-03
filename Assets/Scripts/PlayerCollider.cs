using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerCollider : MonoBehaviour
{
    private bool m_IsOnGround;

    public bool IsOnGround
    {
        get
        {
            return m_IsOnGround;
        }
    }

    private void FixedUpdate()
    {
        Rigidbody body;
        if ((body = GetComponent<Rigidbody>()) != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, transform.localScale.y + 0.05f))
            {
                m_IsOnGround = true;
            }
            else
            {
                m_IsOnGround = false;
            }
        }
    }
}
