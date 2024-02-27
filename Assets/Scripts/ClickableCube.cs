using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableCube : MonoBehaviour
{
    public PopupManager popupManager; // Assign this in the inspector

    private void OnMouseDown()
    {
        // Call the HandleClick method on your PopupManager
        popupManager.HandleClick();
    }
}

