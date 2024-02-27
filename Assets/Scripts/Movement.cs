using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Movement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // Adjust this to control the movement speed.

    private void Update()
    {
        // Get input from the arrow keys or other input methods.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction.
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);

        // Normalize the direction to maintain a consistent speed when moving diagonally.
        movement.Normalize();

        // Move the player by adding the movement vector to its position.
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
*/