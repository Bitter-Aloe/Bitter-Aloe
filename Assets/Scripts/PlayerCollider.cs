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
            if (m_IsOnGround)
            {
                m_IsOnGround = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    void OnCollisionEnter()
    {
        m_IsOnGround = true;
    }
}
