using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidLandController : MonoBehaviour
{

    [SerializeField] private float _speed = 5;
    // Start is called before the first frame update

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

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
        transform.Translate(movement * _speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.None;
        if (Input.GetKeyUp(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.Locked;
    }
}
