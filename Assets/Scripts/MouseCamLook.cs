using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCamLook : MonoBehaviour
{

    [SerializeField]
    public float sensitivity = 5.0f;
    [SerializeField]
    public float smoothing = 2.0f;
    [SerializeField]
    public float vertAngle = 45.0f;

    // the chacter is the capsule
    public GameObject character;
    public new GameObject camera;
    // get the incremental value of mouse moving
    private Vector2 mouseLook;
    // smooth the mouse moving
    private Vector2 smoothV;



    // Update is called once per frame
    void Update()
    {
        if(Pausing.gameIsPaused) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // md is mosue delta
            var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
            // incrementally add to the camera look

            mouseLook += smoothV;
            mouseLook.y = Mathf.Clamp(mouseLook.y, -vertAngle, vertAngle);
        }
        // vector3.right means the x-axis
        camera.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);


    }
}
