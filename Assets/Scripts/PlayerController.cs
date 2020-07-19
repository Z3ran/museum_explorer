using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject pointOfView;
    private CharacterController controller;

    public Transform groundCheck;
    public float groundDistance = 0.02f;

    public LayerMask groundMask;

    public float mouseSensitivity = 100f;
    public float gravity = -20f;
    public float straffSpeed = 1f;

    public float horizontalVelocity = 12f;

    Vector3 velocity;

    float xRotation = 0f;
    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move.Normalize();

        var moveVelocity = horizontalVelocity;

        if (x != 0)
        {
            moveVelocity = moveVelocity * this.straffSpeed;
        }
        if (z < 0)
        {
            // Backward
            moveVelocity = moveVelocity * 0.3f;
        }

        controller.Move(move * moveVelocity * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        pointOfView.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void FixedUpdate()
    {
        
    }
}
