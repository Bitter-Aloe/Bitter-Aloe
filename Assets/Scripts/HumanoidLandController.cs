using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidLandController : MonoBehaviour
{

    [SerializeField] private float _speed = 5;
    [SerializeField] public float sprintFactor = 2f;
    [SerializeField] public float jumpHeight = 5.0f;

    public AudioSource walkSound, sprintSound;

    private PlayerCollider playerCollider;
    private bool shouldJump = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCollider = GetComponent<PlayerCollider>();
    }

    private Vector3 currentMov;
    private float currentSpeed;

    private void Update()
    {
        // Get input from the arrow keys or other input methods.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction.
        currentMov = new Vector3(horizontalInput, 0.0f, verticalInput);

        // Normalize the direction to maintain a consistent speed when moving diagonally.
        currentMov.Normalize();

        // Move the player by adding the movement vector to its position.
        currentSpeed = _speed;
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        if (sprinting)
            currentSpeed = currentSpeed * sprintFactor;

        if (Input.GetKeyDown(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.None;
        if (Input.GetKeyUp(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.Locked;

        bool onGround = playerCollider.IsOnGround;
        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            shouldJump = true;
        }
            

        if(currentMov.magnitude > 0.0f && onGround)
        {
            Debug.Log(currentMov);
            if (sprinting)
            {
                walkSound.Pause();
                sprintSound.UnPause();
            }
            else
            {
                walkSound.UnPause();
                sprintSound.Pause();
            }
        }
        else
        {
            walkSound.Pause();
            sprintSound.Pause();
        }

    } 

    private void FixedUpdate()
    {
        Vector3 vel = GetComponent<Rigidbody>().velocity;
        Vector3 mov = currentMov * currentSpeed;
        Vector3 newVel = mov.x * transform.right + vel.y * transform.up + mov.z * transform.forward;
        GetComponent<Rigidbody>().velocity = newVel;
        if (shouldJump)
        {
            GetComponent<Rigidbody>().velocity += new Vector3(0, jumpHeight, 0);
            shouldJump = false;
        }
    }


}
