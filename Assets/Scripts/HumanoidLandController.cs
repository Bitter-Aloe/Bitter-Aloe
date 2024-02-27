using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidLandController : MonoBehaviour
{

    [SerializeField] private float _speed = 5;
    [SerializeField] public float sprintFactor = 2f;
    [SerializeField] public float jumpHeight = 5.0f;
    // Start is called before the first frame update

    private PlayerCollider playerCollider;
    private bool shouldJump = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCollider = GetComponent<PlayerCollider>();
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
        float speed = _speed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed = speed * sprintFactor;
        transform.Translate(movement * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.None;
        if (Input.GetKeyUp(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.Locked;

        if (Input.GetKeyDown(KeyCode.Space) && playerCollider.IsOnGround)
            shouldJump = true;
    } 

    private void FixedUpdate()
    {
            if (shouldJump)
            {
                GetComponent<Rigidbody>().velocity += new Vector3(0, jumpHeight, 0);
                shouldJump = false;
            }
    }


}
